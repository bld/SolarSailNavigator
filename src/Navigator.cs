using System;
using System.Linq;
using UnityEngine;
using PersistentThrust;

namespace SolarSailNavigator {

    public class PersistentControlled : PersistentEngine {

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
			ThrottlePersistent = control.throttle;
			ThrustPersistent = control.throttle * maxThrust;
			IspPersistent = atmosphereCurve.Evaluate(0);
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