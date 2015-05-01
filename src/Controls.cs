using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class SailControl {

	// Fields
	// Cone Angle
	public float cone;
	public string cone_str;
	// Clock angle
	public float clock;
	public string clock_str;
	// Duration of steering maneuver
	public double duration;
	public string duration_str;
	// Warp factor
	public double factor;
	public string factor_str;
	// Controls object this control is attached to
	public SailControls controls;
	// Sail this controls
	public ModuleSolarSail sail; // Sail this control is attached to

	// Static fields
	public static float defaultCone = 90f;
	public static float defaultClock = 0f;
	public static double defaultDuration = 100000.0;
	public static double defaultFactor = 100000.0;

	// Update sail persistant variables when controls change
	//public void Update() {
	//    sail.cone = cone_str;
	//    sail.clock = clock_str;
	//    sail.duration = duration_str;
	//    sail.factor = factor_str;
	//}
	
	// Tilt controls
	
	public void TiltPlus() {
	    cone += 5;
	    if (cone > 90) {
		cone = cone - 180;
	    }
	}

	public void TiltMinus() {
	    cone -= 5;
	    if (cone < -90) {
		cone = cone + 180;
	    }
	}

	// Rotate controls

	public void RotatePlus() {
	    clock += 5;
	    if (clock > 180) {
		clock = clock - 360;
	    }
	}

	public void RotateMinus() {
	    clock -= 5;
	    if (clock < -180) {
		clock = 360 + clock;
	    }
	}

	// GUI line
	public void GUILine () {
	    // Line
	    GUILayout.BeginHorizontal();
	    // Cone
	    GUILayout.Label(cone_str);
	    if (GUILayout.Button("+")) {
		TiltPlus();
		cone_str = cone.ToString();
	    }
	    if (GUILayout.Button("-")) {
		TiltMinus();
		cone_str = cone.ToString();
	    }
	    // Clock
	    GUILayout.Label(clock_str);
	    if (GUILayout.Button("+")) {
		RotatePlus();
		clock_str = clock.ToString();
	    }
	    if (GUILayout.Button("-")) {
		RotateMinus();
		clock_str = clock.ToString();
	    }
	    // Duration
	    duration_str = GUILayout.TextField(duration_str, 25);
	    double tmpd;
	    if (Double.TryParse(duration_str, out tmpd)) {
		duration = tmpd;
	    }
	    // Warp factor
	    factor_str = GUILayout.TextField(factor_str, 25);
	    double tmpf;
	    if (Double.TryParse(factor_str, out tmpf)) {
		factor = tmpf;
	    }
	    // End line
	    GUILayout.EndHorizontal();
	}

	// Parse a string to a single
	public static float ParseSingle (string str) {
	    float tmp;
	    if (Single.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0.0f;
	    }
	}

	// Parse a string to a double
	public static double ParseDouble (string str) {
	    double tmp;
	    if (Double.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0.0;
	    }
	}
	
	// Constructor

	public SailControl(ModuleSolarSail sail, float cone, float clock, double duration, double factor) {
	    this.sail = sail;
	    this.cone = cone;
	    cone_str = cone.ToString();
	    this.clock = clock;
	    clock_str = clock.ToString();
	    this.duration = duration;
	    duration_str = duration.ToString();
	    this.factor = factor;
	    factor_str = factor.ToString();
	}

	public static SailControl Default (ModuleSolarSail sail) {
	    return new SailControl (sail, defaultCone, defaultClock, defaultDuration, defaultFactor);
	}
    }

    public class SailControls {

	// Fields

	public int ncontrols;
	public SailControl[] controls;
	public ModuleSolarSail sail;
	public double UT0;
	public SailControl sailOff;
	
	// Static fields
	// Delimiter between controls
	private char delimiter = ':';

	// Constructor

	// Give the sail to which this control is for
	public SailControls (ModuleSolarSail sail) {
	    // Assign sail field
	    this.sail = sail;
	    Debug.Log(this.sail.ToString());
	    // Initial time
	    if (sail.UT0 == 0) {
		UT0 = Planetarium.GetUniversalTime();
	    } else {
		UT0 = sail.UT0;
	    }
	    Debug.Log(UT0.ToString());
	    // Off sail control
	    sailOff = SailControl.Default(sail);
	    // Split control strings from sail into arrays
	    if (String.IsNullOrEmpty(sail.cones) ||
		String.IsNullOrEmpty(sail.clocks) ||
		String.IsNullOrEmpty(sail.durations) ||
		String.IsNullOrEmpty(sail.factors)) {
		ncontrols = 1;
		controls = new SailControl[ncontrols];
		controls[0] = SailControl.Default(sail);
	    } else {
		var coneStrings = sail.cones.Split(delimiter);
		var clockStrings = sail.clocks.Split(delimiter);
		var durationStrings = sail.durations.Split(delimiter);
		var factorStrings = sail.factors.Split(delimiter);
		// number of controls
		ncontrols = Math.Min(coneStrings.Length, Math.Min(clockStrings.Length, Math.Min(durationStrings.Length, factorStrings.Length)));
		// initialize controls array
		controls = new SailControl[ncontrols];
		// Populate controls
		for(var i = 0; i < ncontrols; i++) {
		    controls[i] = new SailControl(sail,
						  SailControl.ParseSingle(coneStrings[i]),
						  SailControl.ParseSingle(clockStrings[i]),
						  SailControl.ParseDouble(durationStrings[i]),
						  SailControl.ParseSingle(factorStrings[i]));
		}
	    }
	}
	
	// GUI
	public void GUI() {
	    GUILayout.BeginVertical();
	    // Set the initial time of the sequence
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Start time: " + UT0.ToString());
	    if(GUILayout.Button("Set to Now")) {
		UT0 = Planetarium.GetUniversalTime();
	    }
	    GUILayout.EndHorizontal();
	    // Controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Cone");
	    GUILayout.Label("Clock");
	    GUILayout.Label("Duration");
	    GUILayout.Label("Warp");
	    GUILayout.EndHorizontal();
	    foreach(var control in controls) {
		control.GUILine();
	    }
	    GUILayout.BeginHorizontal();
	    if (GUILayout.Button("Add")) { Add(); };
	    if (GUILayout.Button("Remove")) { Remove(); };
	    GUILayout.EndHorizontal();
	    
	    GUILayout.EndVertical();
	    Update();
	}

	// Add a control
	public void Add () {
	    var newControls = new SailControl[ncontrols + 1];
	    for(var i = 0; i < ncontrols; i++) {
		newControls[i] = controls[i];
	    }
	    newControls[ncontrols] = SailControl.Default(sail);
	    controls = newControls;
	    ncontrols++;
	}
	
	// Remove a control
	public void Remove () {
	    if (ncontrols > 1) {
		var newControls = new SailControl[ncontrols - 1];
		for (var i = 0; i < ncontrols - 1; i++) {
		    newControls[i] = controls[i];
		}
		controls = newControls;
		ncontrols--;
	    }
	}
	
	// Return control at specified UT
	public SailControl Lookup (double UT) {
	    double deltaUT = UT - UT0; // Time past UT0
	    double durationSum = 0.0; // Running sum of durations
	    foreach (var control in controls) {
		// deltaUT between prior and next duration
		if (deltaUT >= durationSum && deltaUT < (durationSum += control.duration)) {
		    return control;
		}
	    }
	    // If none found, return "sail off" control
	    return sailOff;
	}

	// Update the sail's persistant sail control fields
	public void Update () {
	    // Initial time
	    sail.UT0 = UT0;
	    // Controls
	    sail.cones = controls[0].cone_str;
	    sail.clocks = controls[0].clock_str;
	    sail.durations = controls[0].duration_str;
	    sail.factors = controls[0].factor_str;
	    for (var i = 1; i < ncontrols; i++) {
		sail.cones += delimiter + controls[i].cone_str;
		sail.clocks += delimiter + controls[i].clock_str;
		sail.durations += delimiter + controls[i].duration_str;
		sail.factors += delimiter + controls[i].factor_str;
	    }
	    Debug.Log(sail.UT0.ToString());
	    Debug.Log(sail.cones);
	    Debug.Log(sail.clocks);
	    Debug.Log(sail.durations);
	    Debug.Log(sail.factors);
	}
    }
}
