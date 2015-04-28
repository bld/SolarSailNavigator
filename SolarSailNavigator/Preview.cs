using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class PreviewSegment {

	// Fields
	
	public Orbit orbit0;
	public Orbit orbitf;
	public Orbit[] orbits;
	public LineRenderer line;
	public double UT0;
	public double UTf;
	double dT;
	GameObject obj;
	public VectorLine vline;

	public PreviewSegment () { }
	
	public void Calculate (ModuleSolarSail sail) {

	    UT0 = Planetarium.GetUniversalTime();
	    Debug.Log(UT0.ToString());

	    UTf = UT0 + sail.controls.duration;
	    Debug.Log(UTf.ToString());

	    dT = TimeWarp.fixedDeltaTime * sail.controls.factor;
	    Debug.Log(dT.ToString());
	    
	    // Calculate preview orbits

	    orbits = ModuleSolarSail.PropagateOrbit(sail, sail.vessel.orbit, UT0, UTf, dT, sail.controls.coneAngle, sail.controls.clockAngle, sail.vessel.GetTotalMass());
	    orbit0 = orbits[0];
	    orbitf = orbits[orbits.Length - 1];
	    
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
	    line.SetColors(Color.yellow, Color.yellow);
	    line.SetWidth(20000, 20000);
	    line.SetVertexCount(orbits.Length);
	}

	// Update segment during renders

	public void Update(Vessel vessel) {
	    if (line != null) {
		// Enable only on map
		if (MapView.MapIsEnabled) {
		    line.enabled = true;
		    // Update points
		    Vector3d rRefUT0 = vessel.orbit.referenceBody.getPositionAtUT(UT0);
		    for (int i = 0; i < orbits.Length; i++) {
			double UTi = orbits[i].epoch;
			line.SetPosition(i, ScaledSpace.LocalToScaledSpace(orbits[i].getRelativePositionAtUT(UTi).xzy + rRefUT0));
		    }
		} else {
		    line.enabled = false;
		}
	    }
	}
    }
}
