using System;
using System.Linq;
using UnityEngine;

namespace PersistentThrust {

    public class PersistentEngine : ModuleEngines {

	// GUI display values
	// Thrust
	[KSPField(guiActive = true, guiName = "Thrust")]
	protected string Thrust = "";
	// Isp
	[KSPField(guiActive = true, guiName = "Isp")]
	protected string Isp = "";
	// Throttle
	[KSPField(guiActive = true, guiName = "Throttle")]
	protected string Throttle = "";
	// Propellant Usage
	[KSPField(guiActive = true, guiName = "")]
	protected string PropellantUse = "";
	// Other resource usage
	[KSPField(guiActive = true, guiName = "")]
	protected string ResourceUse = "";
	
	// Numeric display values
	protected double thrust_d = 0;
	protected double isp_d = 0;
	protected double throttle_d = 0;
	
	// Persistent values to use during timewarp
	public float IspPersistent = 0;
	public float ThrustPersistent = 0;
	public float ThrottlePersistent = 0;

	// Are we transitioning from timewarp to reatime?
	bool warpToReal = false;

	// Resource used for deltaV and mass calculations
	[KSPField]
	public string resourceDeltaV;
	// Density of resource
	double density;
	// Propellant
	Propellant prop;

	// Resources not used for deltaV
	Propellant[] propOther;
	
	// Update
	public override void OnUpdate() {

	    // When transitioning from timewarp to real update throttle
	    if (warpToReal) {
		vessel.ctrlState.mainThrottle = ThrottlePersistent;
		warpToReal = false;
	    }
	    
	    // Persistent thrust GUI
	    Fields["Thrust"].guiActive = isEnabled;
	    Fields["Isp"].guiActive = isEnabled;
	    Fields["Throttle"].guiActive = isEnabled;
	    Fields["PropellantUse"].guiActive = isEnabled;
	    Fields["ResourceUse"].guiActive = isEnabled;

	    // Update display values
	    Thrust = Utils.FormatThrust(thrust_d);
	    Isp = Math.Round(isp_d, 2).ToString() + " s";
	    Throttle = Math.Round(throttle_d * 100).ToString() + "%";
	}

	// Initialization
	public override void OnLoad(ConfigNode node) {

	    // Run base OnLoad method
	    base.OnLoad(node);

	    // Initialize density of propellant used in deltaV and mass calculations
	    density = PartResourceLibrary.Instance.GetDefinition(resourceDeltaV).density;
	}

	public override void OnStart(StartState state) {

	    // Save propellant used for deltaV and those that aren't
	    if (state != StartState.None && state != StartState.Editor) {
		propOther = new Propellant[propellants.Count - 1];
		var i = 0;
		foreach (var p in propellants) {
		    if (p.name == resourceDeltaV) {
			prop = p;
		    } else {
			propOther[i] = p;
			i++;
		    }
		}

		// Rename GUI name to propellant used for DeltaV
		Fields["PropellantUse"].guiName = resourceDeltaV + " use";

		// Name ResourceUse GUI to other resources
		Fields["ResourceUse"].guiName = "";
		foreach(var p in propOther) {
		    // If multiple resources, put | between them
		    if (Fields["ResourceUse"].guiName != String.Empty) {
			Fields["ResourceUse"].guiName += "|";
		    }
		    // Add name of resource
		    Fields["ResourceUse"].guiName += p.name;
		}
		// Add "use" to the end
		Fields["ResourceUse"].guiName += " use";
		
		Debug.Log(prop.name + " " + prop.ratio + " " + prop.id);
		foreach (var po in propOther) {
		    Debug.Log(po.name + " " + po.ratio + " " + po.id);
		}
	    }

	    // Run base OnStart method
	    base.OnStart(state);
	}

	// Physics update
	public override void OnFixedUpdate() {
	    if (FlightGlobals.fetch != null && isEnabled) {
		// Time step size
		double dT = TimeWarp.fixedDeltaTime;

		// Realtime mode
		if (!this.vessel.packed) {
		    // if not transitioning from warp to real
		    // Update values to use during timewarp
		    if (!warpToReal) {
			IspPersistent = realIsp;
			ThrottlePersistent = vessel.ctrlState.mainThrottle;
			ThrustPersistent = this.CalculateThrust();
			// Update displayed propellant use
			PropellantUse = (prop.currentAmount / dT).ToString("E3") + " U/s";
			// Update non-propulsive resources
			ResourceUse = "";
			foreach (var p in propOther) {
			    if (ResourceUse != String.Empty) {
				ResourceUse += "|";
			    }
			    ResourceUse += (p.currentAmount / dT).ToString("E3");
			}
			ResourceUse += " U/s";
		    }
		} else { // Timewarp mode: perturb orbit using thrust
		    warpToReal = true; // Set to true for transition to realtime
		    double UT = Planetarium.GetUniversalTime(); // Universal time
		    double m0 = this.vessel.GetTotalMass(); // Current mass
		    double mdot = ThrustPersistent / (IspPersistent * 9.81); // Mass burn rate of engine
		    double dm = mdot * dT; // Change in mass over dT
		    double demand = dm / density; // Resource demand
		    bool depleted = false; // Check if resources depleted
		    
		    // Update vessel resource
		    double demandOut = part.RequestResource(resourceDeltaV, demand);

		    // Update displayed demand
		    PropellantUse = (demandOut / dT).ToString("E3") + " U/s";

		    // Resource depleted if demandOut = 0 & demand was > demandOut
		    if (demand > 0 && demandOut == 0) {
			depleted = true;
		    } // Revise dm if demandOut < demand
		    else if (demand > 0 && demand > demandOut) {
			dm = demandOut * density;
		    }

		    // Calculate demand of other resources
		    // Update displayed values of usage rate
		    ResourceUse = "";
		    foreach (var p in propOther) {
			var demandOther = demandOut * p.ratio / prop.ratio;
			var demandOutOther = part.RequestResource(p.id, demandOther);
			// Depleted if any resource 
			if (demandOther > 0 && demandOutOther == 0) {
			    depleted = true;
			}
			// Update displayed resource use
			if (ResourceUse != String.Empty) {
			    ResourceUse += "|";
			}
			ResourceUse += (demandOutOther / dT).ToString("E3");
		    }
		    ResourceUse += " U/s";
		    
		    // Calculate thrust and deltaV if demand output > 0
		    if (!depleted) {
			double m1 = m0 - dm; // Mass at end of burn
			double deltaV = IspPersistent * 9.81 * Math.Log(m0/m1); // Delta V from burn
			Vector3d thrustV = this.part.transform.up; // Thrust direction
			Vector3d deltaVV = deltaV * thrustV; // DeltaV vector
			vessel.orbit.Perturb(deltaVV, UT, dT); // Update vessel orbit
		    }
		    // Otherwise, if throttle is turned on, and demand out is 0, show warning
		    else if (ThrottlePersistent > 0) {
			Debug.Log("Propellant depleted");
			// Return to realtime mode
			TimeWarp.SetRate(0, true);
		    }
		}

		// Update display numbers
		thrust_d = ThrustPersistent;
		isp_d = IspPersistent;
		throttle_d = ThrottlePersistent;
	    }
	}

	// Simulated deltaV and resource use calculation
	// Used for navigation predictions. Also updates m1.
	public static Vector3d CalculateDeltaV (PersistentEngine engine, double dT, float thrust, float isp, double m0, Vector3d up, double m1) {
	    double mdot = thrust / (isp * 9.81);
	    double dm = mdot * dT;
	    m1 = m0 - dm;
	    double deltaV = isp * 9.81 * Math.Log(m0 / m1);
	    Vector3d deltaVV = deltaV * up;
	    return deltaVV;
	}
    }
}
