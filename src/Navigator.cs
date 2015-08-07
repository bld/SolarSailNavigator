using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using PersistentThrust;

namespace SolarSailNavigator {

    public class Navigator : PartModule {

	// Attitude locked
	[KSPField(isPersistant = true)]
	public bool IsLocked = false;
	// Controls
	[KSPField(isPersistant = true)]
	public bool IsControlled = false;
	[KSPField(isPersistant = true)]
	public double UT0;
	[KSPField(isPersistant = true)]
	public string cones;
	[KSPField(isPersistant = true)]
	public string clocks;
	[KSPField(isPersistant = true)]
	public string durations;
	[KSPField(isPersistant = true)]
	public string throttles;

	// Persistent engine controls
	public Controls controls;

	// Engine part modules this controls
	public List<PersistentEngine> engines;

	// Solar sail part modules this controls
	public List<SolarSailPart> sails;
	
	// Show controls
	[KSPEvent(guiActive = true, guiName = "Show Controls", active = true)]
	public void ShowControls() {
	    IsControlled = true;
	    RenderingManager.AddToPostDrawQueue(3, new Callback(controls.DrawControls));
	}

	// Hide controls
	[KSPEvent(guiActive = true, guiName = "Hide Controls", active = false)]
	public void HideControls() {
	    // Remove control window
	    IsControlled = false;
	    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(controls.DrawControls));
	}

	// Initialization
	public override void OnStart(StartState state) {

	    // Base initialization
	    base.OnStart(state);
	    
	    if (state != StartState.None && state != StartState.Editor) {

		// Find sails and persistent engines
		foreach (Part p in vessel.parts) {
		    foreach (PartModule pm in p.Modules) {
			if (pm.ClassName == "SolarSailPart") {
			    sails.Add((SolarSailPart)pm);
			} else if (pm.ClassName == "PersistentEngine") {
			    engines.Add((PersistentEngine)pm);
			}
		    }
		}
				
		// Sail controls
		controls = new Controls(this);
		
		// Draw controls
		if (IsControlled) {
		    RenderingManager.AddToPostDrawQueue(3, new Callback(controls.DrawControls));
		}
	    }
	}

	// Updated
	public override void OnUpdate() {
	    
	    // Base update
	    base.OnUpdate();
	    
	    // Sail deployment GUI
	    Events["ShowControls"].active = !IsControlled;
	    Events["HideControls"].active = IsControlled;
	}

	// Physics update
	public override void OnFixedUpdate() {

	    // Universal time
	    double UT = Planetarium.GetUniversalTime();

	    // Force attitude to specified frame & hold throttle
	    if (FlightGlobals.fetch != null && IsLocked) {
		// Set attitude
		Control control = controls.Lookup(UT);
		vessel.SetRotation(Frames.SailFrame(vessel.orbit, control.cone, control.clock, UT));

		// Set throttle
		if (isEnabled) {
		    // Realtime mode
		    if (!vessel.packed) {
			vessel.ctrlState.mainThrottle = control.throttle;
		    }
		    // Warp mode
		    else {
			foreach (var e in engines) {
			    e.ThrottlePersistent = control.throttle;
			    e.ThrustPersistent = control.throttle * e.maxThrust;
			    e.IspPersistent = e.atmosphereCurve.Evaluate(0);
			}
		    }
		}
	    }

	    // Execute spacecraft dynamics
	    base.OnFixedUpdate();

	    // Update preview trajectory if it exists
	    controls.preview.Update(vessel);	    
	}
    }
}