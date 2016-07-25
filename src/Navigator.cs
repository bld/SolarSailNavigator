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
	// Control parameters by segment
	[KSPField(isPersistant = true)] // Steering frame
	public string frames;
	[KSPField(isPersistant = true)] // Euler angles 0
	public string angle0s;
	[KSPField(isPersistant = true)] // Euler angles 1
	public string angle1s;
	[KSPField(isPersistant = true)] // Euler angles 2
	public string angle2s;
	[KSPField(isPersistant = true)] // Duration in seconds
	public string durations;
	[KSPField(isPersistant = true)] // Throttle
	public string throttles;
	[KSPField(isPersistant = true)] // Is sail on?
	public string sailons;
	// Default control parameters
	[KSPField(isPersistant = true)] // Steering frame
	public string defaultFrame = "RTN";
	[KSPField(isPersistant = true)] // Euler angles 0
	public float defaultAngle0 = Frame.Frames["RTN"].defaults[0];
	[KSPField(isPersistant = true)] // Euler angles 1
	public float defaultAngle1 = Frame.Frames["RTN"].defaults[1];
	[KSPField(isPersistant = true)] // Euler angles 2
	public float defaultAngle2 = Frame.Frames["RTN"].defaults[2];
	[KSPField(isPersistant = true)] // Days
	public double defaultDays = 10.0;
	[KSPField(isPersistant = true)] // Hours
	public double defaultHours = 0.0;
	[KSPField(isPersistant = true)] // Throttle
	public float defaultThrottle = 0f;
	[KSPField(isPersistant = true)] // Is sail on?
	public bool defaultSailon = true;
	[KSPField(isPersistant = true)] // Is sail on?
	public int defaultIWarp = 10;

	// Are there any persistent engines or sails?
	bool anyPersistent;
	
	// Persistent engine controls
	public Controls controls;

	// Engine part modules this controls
	public List<PersistentEngine> persistentEngines;

	// Solar sail part modules this controls
	public List<SolarSailPart> solarSails;

	// Show controls
	[KSPEvent(guiActive = true, guiName = "Show Navigator Controls", active = true)]
	public void ShowControls() {
	    IsControlled = true;
	    //RenderingManager.AddToPostDrawQueue(3, new Callback(controls.DrawControls));
	}

	// Hide controls
	[KSPEvent(guiActive = true, guiName = "Hide Navigator Controls", active = false)]
	public void HideControls() {
	    // Remove control window
	    IsControlled = false;
	    //RenderingManager.RemoveFromPostDrawQueue(3, new Callback(controls.DrawControls));
	}

	// When to draw Controls GUI
	private void OnGUI() {
	    if (IsControlled & anyPersistent) {
		controls.DrawControls();
		controls.defaultWindow.DrawWindow();
		foreach (var control in controls.controls) {
		    control.frameWindow.DrawFrameWindow();
		}
	    }
	}
	
	// Initialization
	public override void OnStart(StartState state) {

	    // Base initialization
	    base.OnStart(state);
	    
	    if (state != StartState.None && state != StartState.Editor) {

		// Find sails and persistent engines
		foreach (Part p in vessel.parts) {
		    foreach (PartModule pm in p.Modules) {
			if (pm is SolarSailPart) {
			    solarSails.Add((SolarSailPart)pm);
			} else if (pm is PersistentEngine) {
			    var pm2 = (PersistentEngine)pm;
			    if (pm2.IsPersistentEngine) {
				persistentEngines.Add((PersistentEngine)pm);
			    }
			}
		    }
		}
				
		if (solarSails.Count > 0 || persistentEngines.Count > 0) {
		    // Persistent propulsion found
		    anyPersistent = true;
		    
		    // Sail controls
		    controls = new Controls(this);
		    
		    // Draw controls
		    /*
		    if (IsControlled) {
			RenderingManager.AddToPostDrawQueue(3, new Callback(controls.DrawControls));
		    }
		    */
		} else {
		    anyPersistent = false;
		    Events["ShowControls"].active = false;
		    Events["HideControls"].active = false;
		}
	    }
	}

	// Updated
	public override void OnUpdate() {
	    
	    // Base update
	    base.OnUpdate();
	    
	    // Sail deployment GUI
	    if (anyPersistent) {
		Events["ShowControls"].active = !IsControlled;
		Events["HideControls"].active = IsControlled;
	    }
	}

	// Physics update
	public void FixedUpdate() {
	    if (anyPersistent) {
		
		// Universal time
		double UT = Planetarium.GetUniversalTime();
		
		// Force attitude to specified frame & hold throttle
		if (FlightGlobals.fetch != null && IsLocked) {
		    // Set attitude
		    Control control = controls.Lookup(UT);
		    vessel.SetRotation(control.frame.qfn(vessel.orbit, UT, control.angles));
		    // Set throttle
		    if (isEnabled) {
			// Realtime mode
			if (!vessel.packed) {
			    vessel.ctrlState.mainThrottle = control.throttle;
			}
			// Warp mode
			else {
			    foreach (var pe in persistentEngines) {
				pe.ThrottlePersistent = control.throttle;
				pe.ThrustPersistent = control.throttle * pe.engine.maxThrust;
				pe.IspPersistent = pe.engine.atmosphereCurve.Evaluate(0);
			    }
			}
		    }
		    // Are sails in use?
		    foreach (var s in solarSails) {
			// Control's "sailon" changes relative to sail's "IsEnabled"
			if (control.sailon != s.IsEnabled) {
			    if (control.sailon) { // Sail on
				s.DeploySail();
			    }
			    else { // Sail not on
				s.RetractSail();
			    }
			}
		    }
		}
	    }

	    // Update preview trajectory if it exists
	    if (anyPersistent) {
		controls.preview.Update(vessel);
	    }
	}
    }
}