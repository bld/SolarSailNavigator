using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    class ModuleSolarSail : PartModule {
	// Persistent Variables
	// Sail enabled
	[KSPField(isPersistant = true)]
	public bool IsEnabled = false;
	// Attitude locked
	[KSPField(isPersistant = true)]
	public bool IsLocked = false;
	// Cone angle
	[KSPField(isPersistant = true)]
	protected float coneAngle_f = 0;
	protected string coneAngle = "";
	// Clock angle
	[KSPField(isPersistant = true)]
	protected float clockAngle_f = 0;
	protected string clockAngle = "";
	
	// Persistent False
	[KSPField]
	public float reflectedPhotonRatio = 1f;
	[KSPField]
	public float surfaceArea; // Surface area of the panel.
	[KSPField]
	public string animName;
	
	// GUI
	// Force and Acceleration
	[KSPField(guiActive = true, guiName = "Force")]
	protected string forceAcquired = "";
	[KSPField(guiActive = true, guiName = "Acceleration")]
	protected string solarAcc = "";
	
	protected Transform surfaceTransform = null;
	protected Animation solarSailAnim = null;
	
	const double kerbin_distance = 13599840256;
	const double thrust_coeff = 9.08e-6;
	
	protected double solar_force_d = 0;
	protected double solar_acc_d = 0;
	protected long count = 0;

	[KSPEvent(guiActive = true, guiName = "Deploy Sail", active = true)]
	public void DeploySail() {
	    if (animName != null && solarSailAnim != null) {
		solarSailAnim[animName].speed = 1f;
		solarSailAnim[animName].normalizedTime = 0f;
		solarSailAnim.Blend(animName, 2f);
	    }
	    IsEnabled = true;
	    // Create control window
	    RenderingManager.AddToPostDrawQueue(3, new Callback(DrawControls));
	}
	
	[KSPEvent(guiActive = true, guiName = "Retract Sail", active = false)]
	public void RetractSail() {
	    if (animName != null && solarSailAnim != null) {
		solarSailAnim[animName].speed = -1f;
		solarSailAnim[animName].normalizedTime = 1f;
		solarSailAnim.Blend(animName, 2f);
	    }
	    IsEnabled = false;
	    // Remove control window
	    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawControls));
	}

	// Tilt controls

	public void TiltPlus() {
	    coneAngle_f += 5;
	    if (coneAngle_f > 90) {
		coneAngle_f = coneAngle_f - 180;
	    }
	}

	public void TiltMinus() {
	    coneAngle_f -= 5;
	    if (coneAngle_f < -90) {
		coneAngle_f = coneAngle_f + 180;
	    }
	}

	// Rotate controls

	public void RotatePlus() {
	    clockAngle_f += 5;
	    if (clockAngle_f > 180) {
		clockAngle_f = clockAngle_f - 360;
	    }
	}

	public void RotateMinus() {
	    clockAngle_f -= 5;
	    if (clockAngle_f < -180) {
		clockAngle_f = 360 + clockAngle_f;
	    }
	}

	// Initialization
	public override void OnStart(StartState state) {
	    if (state != StartState.None && state != StartState.Editor) {
		//surfaceTransform = part.FindModelTransform(surfaceTransformName);
		//solarSailAnim = (ModuleAnimateGeneric)part.Modules["ModuleAnimateGeneric"];
		if (animName != null) {
		    solarSailAnim = part.FindModelAnimators(animName).FirstOrDefault();
		}
		if (IsEnabled) {
		    solarSailAnim[animName].speed = 1f;
		    solarSailAnim[animName].normalizedTime = 0f;
		    solarSailAnim.Blend(animName, 0.1f);
		    RenderingManager.AddToPostDrawQueue(3, new Callback(DrawControls));
		}
		
		this.part.force_activate();
	    }
	}

	public override void OnUpdate() {
	    // Sail deployment GUI
	    Events["DeploySail"].active = !IsEnabled;
	    Events["RetractSail"].active = IsEnabled;
	    // Text fields (acc & force)
	    Fields["solarAcc"].guiActive = IsEnabled;
	    Fields["forceAcquired"].guiActive = IsEnabled;
	    forceAcquired = solar_force_d.ToString("E") + " N";
	    solarAcc = solar_acc_d.ToString("E") + " m/s";
	    // Tilt/rotate control strings
	    coneAngle = coneAngle_f.ToString() + " deg";
	    clockAngle = clockAngle_f.ToString() + " deg";
	}
	
	public override void OnFixedUpdate() {
	    if (FlightGlobals.fetch != null) {

		double UT = Planetarium.GetUniversalTime();
		
		solar_force_d = 0;
		if (!IsEnabled) { return; }

		// Force attitude to sail frame
		if (IsLocked) {
		    // vessel.SetRotation(SailFrame(vessel, coneAngle_f, clockAngle_f));
		    vessel.SetRotation(SailFrame(vessel.orbit, coneAngle_f, clockAngle_f, UT));
		}
		
		double sunlightFactor = 1.0;
		//Vector3 sunVector = FlightGlobals.fetch.bodies[0].position - part.orgPos;
		
//		if (!lineOfSightToSun(vessel)) {
//		    sunlightFactor = 0.0f;
//		}

		if (!inSun(vessel.orbit, UT)) {
		    sunlightFactor = 0.0;
		}
		
		//Debug.Log("Detecting sunlight: " + sunlightFactor.ToString());
		Vector3d solarForce = CalculateSolarForce() * sunlightFactor;
		//print(surfaceArea);
		
		Vector3d solar_accel = solarForce / vessel.GetTotalMass() / 1000.0 * TimeWarp.fixedDeltaTime;
		if (!this.vessel.packed) {
		    vessel.ChangeWorldVelocity(solar_accel);
		} else {
		    if (sunlightFactor > 0) {
			double temp1 = solar_accel.y;
			solar_accel.y = solar_accel.z;
			solar_accel.z = temp1;
			Vector3d position = vessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());
			Orbit orbit2 = new Orbit(vessel.orbit.inclination, vessel.orbit.eccentricity, vessel.orbit.semiMajorAxis, vessel.orbit.LAN, vessel.orbit.argumentOfPeriapsis, vessel.orbit.meanAnomalyAtEpoch, vessel.orbit.epoch, vessel.orbit.referenceBody);
			orbit2.UpdateFromStateVectors(position, vessel.orbit.vel + solar_accel, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
			//print(orbit2.timeToAp);
			if (!double.IsNaN(orbit2.inclination) && !double.IsNaN(orbit2.eccentricity) && !double.IsNaN(orbit2.semiMajorAxis) && orbit2.timeToAp > TimeWarp.fixedDeltaTime) {
			    vessel.orbit.inclination = orbit2.inclination;
			    vessel.orbit.eccentricity = orbit2.eccentricity;
			    vessel.orbit.semiMajorAxis = orbit2.semiMajorAxis;
			    vessel.orbit.LAN = orbit2.LAN;
			    vessel.orbit.argumentOfPeriapsis = orbit2.argumentOfPeriapsis;
			    vessel.orbit.meanAnomalyAtEpoch = orbit2.meanAnomalyAtEpoch;
			    vessel.orbit.epoch = orbit2.epoch;
			    vessel.orbit.referenceBody = orbit2.referenceBody;
			    vessel.orbit.Init();
			    
			    //vessel.orbit.UpdateFromOrbitAtUT(orbit2, Planetarium.GetUniversalTime(), orbit2.referenceBody);
			    vessel.orbit.UpdateFromUT(Planetarium.GetUniversalTime());
			}

		    }
		}
		solar_force_d = solarForce.magnitude;
		solar_acc_d = solar_accel.magnitude / TimeWarp.fixedDeltaTime;
		//print(solarForce.x.ToString() + ", " + solarForce.y.ToString() + ", " + solarForce.z.ToString());

	    }
	    count++;
	}

	private Vector3d CalculateSolarForce() {
	    if (this.part != null) {
		Vector3d sunPosition = FlightGlobals.fetch.bodies[0].position;
		Vector3d ownPosition = this.part.transform.position;
		Vector3d ownsunPosition = ownPosition - sunPosition;
		Vector3d normal = this.part.transform.up;
		if (surfaceTransform != null) {
		    normal = surfaceTransform.forward;
		}
		// If normal points away from sun, negate so our force is always away from the sun
		// so that turning the backside towards the sun thrusts correctly
		if (Vector3d.Dot (normal, ownsunPosition) < 0) {
		    normal = -normal;
		}
		// Magnitude of force proportional to cosine-squared of angle between sun-line and normal
		double cosConeAngle = Vector3.Dot (ownsunPosition.normalized, normal);
		Vector3d force = normal * cosConeAngle * cosConeAngle * surfaceArea * reflectedPhotonRatio * solarForceAtDistance();
		return force;
	    } else {
		return Vector3d.zero;
	    }
	}

	private double solarForceAtDistance() {
	    double distance_from_sun = Vector3.Distance(FlightGlobals.Bodies[0].transform.position, vessel.transform.position);
	    double force_to_return = thrust_coeff * kerbin_distance * kerbin_distance / distance_from_sun / distance_from_sun;
	    return force_to_return;
	}

	public static Quaternion RTNFrame (Vessel vessel) {
	    // Center of mass position
	    var CM = vessel.findWorldCenterOfMass();
	    // Unit position vector
	    var r = (CM - vessel.mainBody.position).normalized;
	    // Unit velocity vector
	    var v = vessel.obt_velocity.normalized;
	    // Unit orbital angular velocity
	    var h = Vector3d.Cross(r, v).normalized;
	    // Tangential vector
	    var t = Vector3d.Cross(h, r).normalized;
	    // Quaternion of RTN frame
	    return Quaternion.LookRotation(t, r);
	}

	public static Quaternion SailFrame (Vessel vessel, float cone, float clock) {
	    var QRTN = RTNFrame(vessel);
	    var QCC = Quaternion.Euler(0, 90 - clock, cone);
	    return QRTN * QCC;
	}
	
	private Rect controlWindowPos = new Rect();

	private void DrawControls () {
	    if (this.vessel == FlightGlobals.ActiveVessel)
		controlWindowPos = GUILayout.Window(10, controlWindowPos, SailControlsGUI, "Sail Controls");
	}

	private void SailControlsGUI (int WindowID) {

	    GUILayout.BeginVertical();

	    // Lock/Unlock attitude
	    IsLocked = GUILayout.Toggle(IsLocked, "Lock Attitude");

	    // Cone angle controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Cone angle");
	    GUILayout.Label(coneAngle);
	    if (GUILayout.Button("+")) {
		TiltPlus();
	    }
	    if (GUILayout.Button("-")) {
		TiltMinus();
	    }
	    GUILayout.EndHorizontal();
	    
	    // Clock angle controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Clock angle");
	    GUILayout.Label(clockAngle);
	    if (GUILayout.Button("+")) {
		RotatePlus();
	    }
	    if (GUILayout.Button("-")) {
		RotateMinus();
	    }
	    GUILayout.EndHorizontal();
	    
	    GUILayout.EndVertical();

	    GUI.DragWindow();
	}

	// Calculate RTN frame quaternion given an orbit and UT
	public static Quaternion RTNFrame (Orbit orbit, double UT) {
	    // Position
	    var r = transposeYZ(orbit.getRelativePositionAtUT(UT).normalized);
	    // Velocity
	    var v = transposeYZ(orbit.getOrbitalVelocityAtUT(UT).normalized);
	    // Unit orbit angular momentum
	    var h = Vector3d.Cross(r, v).normalized;
	    // Tangential
	    var t = Vector3d.Cross(h, r).normalized;
	    // QRTN
	    return Quaternion.LookRotation(t, r);
	}

	// Sail frame in local (RTN) coordinates
	public static Quaternion SailFrameLocal (float cone, float clock) {
	    return Quaternion.Euler(0, 90 - clock, cone);
	}
	
	// Sail frame given an orbit, angles, and UT
	public static Quaternion SailFrame (Orbit orbit, float cone, float clock, double UT) {
	    var QRTN = RTNFrame(orbit, UT);
	    var QCC = SailFrameLocal(cone, clock);
	    return QRTN * QCC;
	}

	// Test if an orbit at UT is in sunlight
	public static bool inSun(Orbit orbit, double UT) {
	    Vector3d a = orbit.getPositionAtUT(UT);
	    Vector3d b = FlightGlobals.Bodies[0].getPositionAtUT(UT);
	    foreach (CelestialBody referenceBody in FlightGlobals.Bodies) {
		if (referenceBody.flightGlobalsIndex == 0) { // the sun should not block line of sight to the sun
		    continue;
		}
		Vector3d refminusa = referenceBody.getPositionAtUT(UT) - a;
		Vector3d bminusa = b - a;
		if (Vector3d.Dot(refminusa, bminusa) > 0) {
		    if (Vector3d.Dot(refminusa, bminusa.normalized) < bminusa.magnitude) {
			Vector3d tang = refminusa - Vector3d.Dot(refminusa, bminusa.normalized) * bminusa.normalized;
			if (tang.magnitude < referenceBody.Radius) {
			    return false;
			}
		    }
		}
	    }
	    return true;
	}
	
	// Transpose X and Y elements for conversion of Orbit vector3d
	public static Vector3d transposeYZ (Vector3d v) {
	    return new Vector3d(v.x, v.z, v.y);
	}
    }
}

