using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PersistentThrust;

namespace SolarSailNavigator {

    public class PreviewSegment {

	// Fields
	
	public Orbit orbit0; // Initial orbit of segment
	public Orbit orbitf; // Final orbit of segment
	public List<Orbit> orbits; // Intermediate segment orbits
	public Vector3d[] relativePoints; // Relative points along orbit
	public LineRenderer line; // Line drawing sail trajectory
	public double UT0; // Initial time of segment
	public double UTf; // Final time of segment
	double dT; // Step size
	GameObject obj; // Game object of line
	public double m0; // Initial mass
	public double m1; // Final mass
	
	// Constructor & calculate

	public void Propagate(Navigator navigator, Orbit orbit0, double UT0, double UTf, double dT, Control control, double m0in) {

	    // Control parameters
	    var throttle = control.throttle;
	    var sailon = control.sailon;
	    var frame = control.frame;
	    
	    // Update segment initial mass
	    m0 = m0in;
	    
	    // Working orbit at each time step
	    Orbit orbit = orbit0.Clone();

	    // Number of time steps
	    int nsteps = Convert.ToInt32(Math.Ceiling((UTf - UT0) / dT));

	    // Last time step size
	    double dTlast = (UTf - UT0) % dT;

	    // Current universal time
	    double UT;

	    // Current mass
	    double m0i = m0;

	    // Reseting time step to sample orbits for saving
	    double dTchoose = 0.0;

	    // List of orbits to preview
	    orbits = new List<Orbit>();

	    // Add initial orbit
	    orbits.Add(orbit0.Clone());

	    // Iterate for nsteps
	    for (int i = 0; i < nsteps; i++) {
		
		// Last step goes to UTf
		if (i == nsteps - 1) {
		    dT = dTlast;
		    UT = UTf;
		} else {
		    UT = UT0 + i * dT;
		}
		
		// Spacecraft reference frame
		Quaternion sailFrame = frame.qfn(orbit, UT, control.angles);

		// Total deltaV vector
		Vector3d deltaVV = new Vector3d(0.0, 0.0, 0.0);

		// Accumulated mass change for all engines
		double dms = 0.0;
		
		// Iterate over engines
		foreach (var pe in navigator.persistentEngines) {

		    // Only count thrust of engines that are not shut down in preview
		    if (pe.engine.getIgnitionState) {

			// Thrust unit vector
			Vector3d thrustUV = sailFrame * new Vector3d(0.0, 1.0, 0.0);
			
			// Isp: Currently vacuum. TODO: calculate at current air pressure
			float isp = pe.engine.atmosphereCurve.Evaluate(0);
			
			// Thrust vector
			float thrust = throttle * pe.engine.maxThrust;

			// Calculate deltaV vector
			double demandMass;
			deltaVV += pe.CalculateDeltaVV(m0i, dT, thrust, isp, thrustUV, out demandMass);

			// Update mass usage
			dms += demandMass * pe.densityAverage;
		    }
		}
		
		// Iterate over sails
		if (sailon) {
		    foreach (var s in navigator.solarSails) {
			
			// Check if sail in sun
			double sunlightFactor = 1.0;
			if (!SolarSailPart.inSun(orbit, UT)) {
			    sunlightFactor = 0.0;
			}
			
			// Normal vector
			Vector3d n = sailFrame * new Vector3d(0.0, 1.0, 0.0);
			
			// Force on sail
			Vector3d solarForce = SolarSailPart.CalculateSolarForce(s, orbit, n, UT) * sunlightFactor;
			
			// Sail acceleration
			Vector3d solarAccel = solarForce / m0i / 1000.0;
			
			// Update deltaVV
			deltaVV += solarAccel * dT;
		    }
		}

		// Update starting mass for next time step
		m0i -= dms;

		// Update 

		// Update orbit
		orbit.Perturb(deltaVV, UT);

		// Increment time step at which to sample orbits
		dTchoose += dT;

		// Orbit period
		double TP = orbit.period;
		
		// Decide whether to add orbit to list of orbits to draw
		if (i == nsteps - 1) { // Always add last orbit
		    orbits.Add(orbit.Clone());
		} else if (dTchoose >= TP / 360) { // If 1/360th of current period passed, add orbit
		    orbits.Add(orbit.Clone());
		    // Reset dTchoose
		    dTchoose = 0.0;
		}
	    }
	    
	    // Update final mass
	    m1 = m0i;
	}
	
	public PreviewSegment(Navigator navigator, Orbit orbitInitial, double UT0, double UTf, Control control, Color color, double m0in) {
	    this.UT0 = UT0;
	    this.UTf = UTf;
	    dT = TimeWarp.fixedDeltaTime * control.warp;
	    
	    // Update preview orbits
	    this.Propagate(navigator, orbitInitial, UT0, UTf, dT, control, m0in);
	    orbit0 = orbits[0];
	    orbitf = orbits.Last();

	    // Initialize LineRenderer

	    // Remove old line
	    if (line != null) {
		UnityEngine.Object.Destroy(line);
	    }
	    // Create new one
	    obj = new GameObject("Preview segment");
	    line = obj.AddComponent<LineRenderer>();
	    line.useWorldSpace = false;
	    obj.layer = 10; // Map view
	    line.material = MapView.fetch.orbitLinesMaterial;
	    line.SetColors(color, color);
	    line.SetVertexCount(orbits.Count);
	    // Calculate relative position vectors
	    relativePoints = new Vector3d[orbits.Count];
	    for(var i = 0; i < orbits.Count; i++) {
		double UTi = orbits[i].epoch;
		relativePoints[i] = orbits[i].getRelativePositionAtUT(UTi).xzy;
	    }
	}

	// Update segment during renders

	public void Update (Vessel vessel) {
	    if (line != null) {
		// Enable only on map
		if (MapView.MapIsEnabled) {
		    line.enabled = true;
		    // Update points
		    Vector3d rRefUT0 = vessel.orbit.referenceBody.getPositionAtUT(UT0);
		    for (int i = 0; i < orbits.Count; i++) {
			line.SetPosition(i, ScaledSpace.LocalToScaledSpace(rRefUT0 + relativePoints[i]));
		    }
		    line.SetWidth(0.01f * MapView.MapCamera.Distance, 0.01f * MapView.MapCamera.Distance);
		} else {
		    line.enabled = false;
		}
	    }
	}
    }
    
    public class Preview {
	
	// Fields
	PreviewSegment[] segments; // Trajectory segments
	Navigator navigator; // Navigator this preview is attached to
	public LineRenderer linef; // Final orbit line
	Vector3d[] linefPoints; // 3d points of final orbit
	double UTf; // final time of trajectory
	public LineRenderer lineT; // Line to target
	Orbit orbitT; // Target object orbit
	public Orbit orbitf; // Final orbit
	
	// Target
	Vector3d rFinalRel; // Relative position of spacecraft at end of control sequence
	Vector3d rTargetFinalRel; // Relative position of target at end of control sequence
	public double targetT; // Elapsed time to closest approach
	public double targetD; // Distance to closest approach
	public double targetV; // Relative speed to target
	public double ApErr; // Error in apoapsis
	public double PeErr; // Error in periapsis
	public double TPErr; // Error in orbital period
	public double IncErr; // Error in inclination
	public double EccErr; // Error in eccentricity
	public double LANErr; // Error in longitude of ascending node
	public double AOPErr; // Error in argument of periapsis
	
	// Constructor
	
	public Preview(Navigator navigator) {
	    this.navigator = navigator;
	}

	// Calculate & draw trajectory prediction

	public void Destroy () {
	    // Destroy trajectory lines
	    if (segments != null) {
		foreach(var segment in segments) {
		    if (segment.line != null) {
			UnityEngine.Object.Destroy(segment.line);
		    }
		}
	    }
	    // Destroy final line
	    if (linef != null) {
		UnityEngine.Object.Destroy(linef);
	    }
	    // Destroy target line
	    if (lineT != null) {
		UnityEngine.Object.Destroy(lineT);
	    }
	}

	public void CalculateTargetLine () {
	    // Selected target
	    var target = FlightGlobals.fetch.VesselTarget;
	    // If a target is selected...
	    if (target != null) {
		orbitT = target.GetOrbit(); // Target orbit
		 // Spacecraft relative position at UTf
		rFinalRel = orbitf.getRelativePositionAtUT(UTf).xzy;
		// Target relative position at UTf
		rTargetFinalRel = orbitT.getRelativePositionAtUT(UTf).xzy;
		// Distance to target at UTf
		targetD = Vector3d.Distance(rFinalRel, rTargetFinalRel);
		// Relative speed to target at UTf
		targetV = Vector3d.Distance(orbitf.getOrbitalVelocityAtUT(UTf), orbitT.getOrbitalVelocityAtUT(UTf));
		// Destroy current line if present
		if (lineT != null) {
		    UnityEngine.Object.Destroy(lineT);
		}
		// Make line to target at UTf
		var objT = new GameObject("Line to target");
		lineT = objT.AddComponent<LineRenderer>();
		lineT.useWorldSpace = false;
		objT.layer = 10; // Map
		lineT.material = MapView.fetch.orbitLinesMaterial;
		lineT.SetColors(Color.red, Color.red);
		lineT.SetVertexCount(2);

		// Target errors
		ApErr = orbitf.ApR - orbitT.ApR;
		PeErr = orbitf.PeR - orbitT.PeR;
		TPErr = orbitf.period - orbitT.period;
		IncErr = orbitf.inclination - orbitT.inclination;
		EccErr = orbitf.eccentricity - orbitT.eccentricity;
		LANErr = orbitf.LAN - orbitT.LAN;
		AOPErr = orbitf.argumentOfPeriapsis - orbitT.argumentOfPeriapsis;
	    }
	}
	
	public void Calculate () {
	    if (navigator.controls.showPreview) {
		// Destroy existing lines
		if (segments != null) {
		    foreach(var segment in segments) {
			if (segment.line != null) {
			    UnityEngine.Object.Destroy(segment.line);
			}
		    }
		}
		// New segments array
		segments = new PreviewSegment[navigator.controls.ncontrols];
		// Beginning time
		double UT0 = navigator.controls.UT0;
		Orbit orbitInitial = navigator.vessel.orbit;
		// Initial mass per segment
		double m0i = navigator.vessel.GetTotalMass();
		// Calculate each segment
		for (var i = 0; i < segments.Length; i++) {
		    // End time
		    double UTf = UT0 + navigator.controls.controls[i].duration;
		    // Calculate segment
		    segments[i] = new PreviewSegment(navigator, orbitInitial, UT0, UTf, navigator.controls.controls[i], navigator.controls.controls[i].color, m0i);
		    // Update initial mass for next segment
		    m0i = segments[i].m1;
		    // Update initial time
		    UT0 = UTf;
		    // Update initial orbit
		    orbitInitial = segments[i].orbitf;
		}

		// Final time of trajectory
		this.UTf = UT0;

		// Draw one complete final orbit
		// Destroy existing line
		if (linef != null) {
		    UnityEngine.Object.Destroy(linef);
		}
		// Create linerenderer & object
		var objf = new GameObject("Final orbit");
		linef = objf.AddComponent<LineRenderer>();
		linef.useWorldSpace = false;
		objf.layer = 10; // Map
		linef.material = MapView.fetch.orbitLinesMaterial;
		linef.SetColors(navigator.controls.colorFinal, navigator.controls.colorFinal);
		linef.SetVertexCount(361);
		// 3D points to use in linef
		linefPoints = new Vector3d[361];
		// Final orbit of trajectory
		orbitf = orbitInitial;
		// Period of final orbit
		double TPf = orbitf.period;
		// Populate points
		for(var i = 0; i <= 360; i++) {
		    double UTi = this.UTf + i * TPf / 360;
		    // Relative orbitf position
		    Vector3d rRelOrbitf = orbitf.getRelativePositionAtUT(UTi).xzy;
		    // Absolute position
		    linefPoints[i] = rRelOrbitf;
		}
		
		// Target line
		CalculateTargetLine();
	    }
	}

	// Update
	public void Update (Vessel vessel) {
	    if (navigator.controls.showPreview) {
		if (segments != null) {
		    foreach(var segment in segments) {
			segment.Update(vessel);
		    }
		}

		// Update final orbit line from points
		if (linef != null) {
		    if (MapView.MapIsEnabled) {
			linef.enabled = navigator.controls.showFinal;
			// Position of reference body at end of trajectory
			Vector3d rRefUTf = vessel.orbit.referenceBody.getPositionAtUT(UTf);
			for (var i = 0; i <= 360; i++) {
			    linef.SetPosition(i, ScaledSpace.LocalToScaledSpace(rRefUTf + linefPoints[i]));
			}
			linef.SetWidth(0.01f * MapView.MapCamera.Distance, 0.01f * MapView.MapCamera.Distance);

			// Update target line
			if (lineT != null) { // If target line exists
			    if (FlightGlobals.fetch.VesselTarget != null) {
				lineT.enabled = true;
				lineT.SetPosition(0, ScaledSpace.LocalToScaledSpace(rRefUTf + rFinalRel));
				lineT.SetPosition(1, ScaledSpace.LocalToScaledSpace(rRefUTf + rTargetFinalRel));
				lineT.SetWidth(0.01f * MapView.MapCamera.Distance, 0.01f * MapView.MapCamera.Distance);
			    } else {
				lineT.enabled = false;
			    }
			}
		    } else {
			linef.enabled = false;
		    }
		}
	    }
	}
    }
}