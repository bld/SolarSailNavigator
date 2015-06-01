using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class SolarSailControlled : SolarSailPart {

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

	// Sail controls
	public SailControls controls;

	// Draw controls window on sail deployment
	[KSPEvent(guiActive = true, guiName = "Show Controls", active = true)]
	public void ShowControls() {
	    // Create control window
	    IsControlled = true;
	    RenderingManager.AddToPostDrawQueue(3, new Callback(controls.DrawControls));
	}
	
	// Stop drawing controls window on sail retraction
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
		controls = new SailControls(this);
		
		// Draw controls
		if (IsControlled) {
		    RenderingManager.AddToPostDrawQueue(3, new Callback(controls.DrawControls));
		}
	    }
	}

	// Updates
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
	    
	    // Force attitude to sail frame
	    if (FlightGlobals.fetch != null && IsLocked) {
		SailControl control = controls.Lookup(UT);
		vessel.SetRotation(Frames.SailFrame(vessel.orbit, control.cone, control.clock, UT));
	    }

	    // Execute sail dynamics
	    base.OnFixedUpdate();

	    // Update preview orbit if it exists
	    controls.preview.Update(vessel);
	}
    }
}
