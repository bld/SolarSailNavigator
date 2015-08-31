using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PersistentThrust;

namespace SolarSailNavigator {

    public class Control {

	// Fields
	// Reference frame
	public Frame frame;
	// Cone Angle
	public float cone;
	public string cone_str;
	// Clock angle
	public float clock;
	public string clock_str;
	// Flatspin angle
	public float flatspin;
	public string flatspin_str;
	// Throttle
	public float throttle;
	public string throttle_str;
	// Sail on
	public bool sailon;
	public string sailon_str;
	// Duration of steering maneuver
	public double duration;
	public string duration_str;
	// Time
	double days;
	string days_str;
	double hours;
	string hours_str;
	// Warp factor
	public int iwarp; // Index of warpLevels array
	public double warp; // Warp factor
	// Color
	public Color color;
	// Parent controls object
	public Controls controls;
	// Parent navigator object
	public Navigator navigator;

	// Static fields
	public static float defaultCone = 90f;
	public static float defaultClock = 0f;
	public static float defaultFlatspin = 0f;
	public static float defaultThrottle = 0f;
	public static bool defaultSailon = true;
	public static double SecondsPerDay = 21600.0;
	public static double SecondsPerHour = 3600.0;
	public static double SecondsPerMinute = 60.0;
	public static double HoursPerDay = 6.0;
	public static double defaultDuration = 10 * SecondsPerDay;
	public static double[] warpLevels = { 1, 2, 3, 4, 5, 10, 50, 100, 1000, 10000, 100000 };
	public static int defaultiwarp = 10;

	// Normalize angles between -180 and 180 degrees
	public float normalizeAngle (float angle) {
	    if (angle < -180 || angle > 180) {
		return (angle + 180.0f) % 360.0f - 180.0f;
	    } else {
		return angle;
	    }
	}
	
	// Cone controls
	public void GUICone () {
	    // Text field
	    string new_str = GUILayout.TextField(cone_str, GUILayout.Width(30));
	    if (new_str != cone_str) {
		cone_str = new_str;
		float parsedValue;
		if (Single.TryParse(cone_str, out parsedValue)) {
		    cone = normalizeAngle(parsedValue);
		    controls.Update();
		}
	    }
	    // +/- buttons
	    if (GUILayout.Button("+")) {
		cone = normalizeAngle(cone + 5);
		cone_str = cone.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		cone = normalizeAngle(cone - 5);
		cone_str = cone.ToString();
		controls.Update();
	    }
	}

	// Clock controls
	public void GUIClock () {
	    // Text field
	    string new_str = GUILayout.TextField(clock_str, GUILayout.Width(30));
	    if (new_str != clock_str) {
		clock_str = new_str;
		float parsedValue;
		if (Single.TryParse(clock_str, out parsedValue)) {
		    clock = normalizeAngle(parsedValue);
		    controls.Update();
		}
	    }
	    // +/- buttons
	    if (GUILayout.Button("+")) {
		clock = normalizeAngle(clock + 5);
		clock_str = clock.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		clock = normalizeAngle(clock - 5);
		clock_str = clock.ToString();
		controls.Update();
	    }
	}

	// Flatspin controls
	public void GUIFlatspin () {
	    // Text field
	    string new_str = GUILayout.TextField(flatspin_str, GUILayout.Width(30));
	    if (new_str != flatspin_str) {
		flatspin_str = new_str;
		float parsedValue;
		if (Single.TryParse(flatspin_str, out parsedValue)) {
		    flatspin = normalizeAngle(parsedValue);
		    controls.Update();
		}
	    }
	    // +/- buttons
	    if (GUILayout.Button("+")) {
		flatspin = normalizeAngle(flatspin + 5);
		flatspin_str = flatspin.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		flatspin = normalizeAngle(flatspin - 5);
		flatspin_str = flatspin.ToString();
		controls.Update();
	    }
	}

	// Throttle controls
	public void GUIThrottle () {
	    GUILayout.BeginVertical();

	    // Throttle controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label(throttle_str, GUILayout.Width(30));
	    if (GUILayout.Button("+")) {
		throttle = (float)Math.Round(throttle + 0.05f, 2);
		if (throttle > 1f) {
		    throttle = 1f;
		}
		throttle_str = throttle.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		throttle = (float)Math.Round(throttle - 0.05f, 2);
		if (throttle < 0f) {
		    throttle = 0f;
		}
		throttle_str = throttle.ToString();
		controls.Update();
	    }
	    GUILayout.EndHorizontal();

	    // Turn sail on/off for this segment
	    if (GUILayout.Toggle(sailon, "Use Sails") != sailon) {
		sailon = !sailon;
		sailon_str = sailon.ToString();
		controls.Update();
	    }

	    GUILayout.EndVertical();
	}

	// Time controls
	public void GUITime () {

	    // Days
	    GUILayout.Label(days_str, GUILayout.Width(30));
	    // Increase
	    GUILayout.BeginVertical();
	    if (GUILayout.Button("+")) {
		days++;
		days_str = days.ToString();
		duration = SecondsPerDay * days + SecondsPerHour * hours;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("+10")) {
		days += 10;
		days_str = days.ToString();
		duration = SecondsPerDay * days + SecondsPerHour * hours;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    GUILayout.EndVertical();
	    // Decrease
	    GUILayout.BeginVertical();
	    if (GUILayout.Button("-")) {
		if (days > 0) {
		    days--;
		    days_str = days.ToString();
		    duration = SecondsPerDay * days + SecondsPerHour * hours;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	    if (GUILayout.Button("-10")) {
		if (days >= 10) {
		    days -= 10;
		    days_str = days.ToString();
		    duration = SecondsPerDay * days + SecondsPerHour * hours;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	    GUILayout.EndVertical();

	    // Hours
	    GUILayout.Label(hours_str, GUILayout.Width(10));
	    // Increase
	    if (GUILayout.Button("+")) {
		hours++;
		if (hours > HoursPerDay) {
		    hours = HoursPerDay;
		}
		hours_str = hours.ToString();
		duration = SecondsPerDay * days + SecondsPerHour * hours;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    // Decrease
	    if (GUILayout.Button("-")) {
		if (hours > 0) {
		    hours--;
		    hours_str = hours.ToString();
		    duration = SecondsPerDay * days + SecondsPerHour * hours;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	}

	// Color controls
	public void GUIColor (Color color) {
	    this.color = color;
	    var cstyle = new GUIStyle();
	    var ctx = new Texture2D(1, 1);
	    cstyle.normal.background = ctx;
	    ctx.SetPixel(1,1,color);
	    ctx.Apply();
	    GUILayout.Label(" ", cstyle, GUILayout.Width(30));
	}
	
	// GUI line
	public void GUILine (Color color, int i) {
	    GUILayout.BeginHorizontal();

	    GUICone();
	    GUIClock();
	    GUIFlatspin();
	    GUIThrottle();
	    GUITime();
	    GUIColor(color);

	    // Add/Remove buttons
	    if (GUILayout.Button("INS")) { controls.Add(i); };
	    if (GUILayout.Button("DEL")) { controls.Remove(i); };
 
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

	// Parse a string to an int
	public static int ParseInt (string str) {
	    int tmp;
	    if (Int32.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0;
	    }
	}

	// Parse a string to a bool
	public static bool ParseBool (string str) {
	    bool tmp;
	    if (Boolean.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return false;
	    }
	}
	
	// Constructor

	public Control(Navigator navigator, Controls controls, float cone, float clock, float flatspin, float throttle, bool sailon, double duration, int iwarp, string frame) {
	    // Navigator
	    this.navigator = navigator;
	    // Parent controls object
	    this.controls = controls;
	    // Reference frame for control angles
	    this.frame = Frame.Frames["RTN"];
	    // Angles
	    this.cone = cone;
	    cone_str = cone.ToString();
	    this.clock = clock;
	    clock_str = clock.ToString();
	    this.flatspin = flatspin;
	    flatspin_str = flatspin.ToString();
	    // Throttle
	    throttle_str = throttle.ToString();
	    this.throttle = throttle;
	    // Sail on
	    sailon_str = sailon.ToString();
	    this.sailon = sailon;
	    // Time
	    this.duration = duration;
	    this.days = Math.Floor(duration / SecondsPerDay);
	    this.days_str = days.ToString();
	    this.hours = Math.Floor(duration % SecondsPerDay / SecondsPerHour);
	    this.hours_str = this.hours.ToString();
	    this.duration = days * SecondsPerDay + hours * SecondsPerHour;
	    duration_str = duration.ToString();
	    // Warp factor
	    this.iwarp = iwarp;
	    warp = warpLevels[iwarp];
	}

	public static Control Default (Navigator navigator, Controls controls) {
	    return new Control(navigator, controls, defaultCone, defaultClock, defaultFlatspin, defaultThrottle, defaultSailon, defaultDuration, defaultiwarp, "RTN");
	}
    }

    public class Controls {

	// Fields

	public int ncontrols;
	public List<Control> controls;
	public Navigator navigator;
	public double UT0;
	public Control navigatorOff;
	double durationTotal;
	public Color colorFinal;
	public bool showPreview = false;
	public bool showFinal = true;
	public bool showFinalElements = false;
	public bool showTargetErr = false;
	bool updateTargetLine = false; // Indicate if target line needs updating
	public Preview preview;
	public string previewButtonText = "Show Preview";
	
	// Static fields
	static Color[] colorMap = { Color.yellow,
				    Color.red,
				    Color.green,
				    Color.magenta,
				    Color.grey,
				    Color.blue,
				    Color.white };

	// Delimiter between controls
	private char delimiter = ':';

	// Constructor

	// Give the navigator to which this control is for
	public Controls (Navigator navigator) {

	    // Assign navigator field
	    this.navigator = navigator;
	    Debug.Log(this.navigator.ToString());

	    // Initial time
	    if (navigator.UT0 == 0) {
		UT0 = Planetarium.GetUniversalTime();
	    } else {
		UT0 = navigator.UT0;
	    }
	    Debug.Log(UT0.ToString());

	    // Off navigator control
	    navigatorOff = Control.Default(navigator, this);

	    // If the navigator doesn't have saved controls, return default
	    if (String.IsNullOrEmpty(navigator.cones) ||
		String.IsNullOrEmpty(navigator.clocks) ||
		String.IsNullOrEmpty(navigator.flatspins) ||
		String.IsNullOrEmpty(navigator.throttles) ||
		String.IsNullOrEmpty(navigator.sailons) ||
		String.IsNullOrEmpty(navigator.durations)) {
		ncontrols = 1;
		controls = new List<Control>();
		controls.Add(Control.Default(navigator, this));

	    } else { // Otherwise, parse saved controls

		// Split into arrays
		var coneStrings = navigator.cones.Split(delimiter);
		var clockStrings = navigator.clocks.Split(delimiter);
		var flatspinStrings = navigator.flatspins.Split(delimiter);
		var throttleStrings = navigator.throttles.Split(delimiter);
		var durationStrings = navigator.durations.Split(delimiter);
		var sailonStrings = navigator.sailons.Split(delimiter);

		// Find number of controls
		ncontrols = Math.Min(Math.Min(coneStrings.Length, clockStrings.Length), Math.Min(durationStrings.Length, Math.Min(flatspinStrings.Length, Math.Min(throttleStrings.Length, sailonStrings.Length))));

		// Initialize controls array
		controls = new List<Control>();

		// Populate controls
		for(var i = 0; i < ncontrols; i++) {
		    controls.Add(new Control(navigator,
					     this,
					     Control.ParseSingle(coneStrings[i]),
					     Control.ParseSingle(clockStrings[i]),
					     Control.ParseSingle(flatspinStrings[i]),
					     Control.ParseSingle(throttleStrings[i]),
					     Control.ParseBool(sailonStrings[i]),
					     Control.ParseDouble(durationStrings[i]),
					     Control.defaultiwarp,
					     "RTN"));
		}
	    }

	    // Preview
	    preview = new Preview(navigator);
	}

	// Convert length in meters to string with bigger units
	string LengthToString(double lengthm) {
	    if (Math.Abs(lengthm) < 1000) {
		return Math.Round(lengthm).ToString() + " m";
	    } else if (Math.Abs(lengthm) < 1000000000) {
		return Math.Round(lengthm / 1000, 3).ToString() + " km";
	    } else {
		return Math.Round(lengthm / 1000000000, 4).ToString() + " Gm";
	    }
	}

	// Convert speed in m/s to string with bigger units
	string SpeedToString(double speedms) {
	    if (Math.Abs(speedms) < 1000) {
		return Math.Round(speedms, 1).ToString() + " m/s";
	    } else {
		return Math.Round(speedms / 1000, 3).ToString() + " km/s";
	    }
	}
	
	// GUI
	public void ControlsGUI(int WindowID) {
	    GUILayout.BeginVertical();

	    // Lock/Unlock attitude
	    navigator.IsLocked = GUILayout.Toggle(navigator.IsLocked, "Lock Attitude");

	    // Set the initial time of the sequence
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Start time: " + UT0.ToString());
	    if(GUILayout.Button("Set to Now")) {
		UT0 = Planetarium.GetUniversalTime();
		Update();
	    }
	    GUILayout.EndHorizontal();

	    // Controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Cone", GUILayout.Width(80));
	    GUILayout.Label("Clock", GUILayout.Width(80));
	    GUILayout.Label("Flatspin", GUILayout.Width(80));
	    GUILayout.Label("Throttle", GUILayout.Width(80));
	    GUILayout.Label("Days", GUILayout.Width(120));
	    GUILayout.Label("Hours", GUILayout.Width(65));
	    GUILayout.Label("Color", GUILayout.Width(30));
	    GUILayout.Label("", GUILayout.Width(80));
	    GUILayout.EndHorizontal();

	    int icolor = 0;
	    durationTotal = 0.0;
	    //foreach(var control in controls) {
	    for (var i = 0; i < ncontrols; i++) {

		var control = controls[i];
		
		// Draw individual GUIs
		control.GUILine(colorMap[icolor], i);
		
		// Update total duration
		durationTotal += control.duration;

		// Update color index
		icolor++;
		if (icolor == colorMap.Length) {
		    icolor = 0;
		}
	    }

	    // Add a control to end
	    if (GUILayout.Button("Add")) { Add(controls.Count); };
	    
	    // Final orbit color
	    colorFinal = colorMap[icolor];

	    // Total duration of sequences
	    GUILayout.Label("Duration: " + durationTotal + " sec");

	    // Preview orbit
	    if (GUILayout.Toggle(showPreview, previewButtonText) != showPreview) {
		showPreview = !showPreview;
	    	if (showPreview) {
		    preview.Calculate();
		} else {
		    preview.Destroy();
		}
	    }
	    
	    // If preview turned on
	    if (showPreview) {
		// Show final orbit
		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(showFinal, "Show Final Orbit") != showFinal) {
		    showFinal = !showFinal;
		    preview.linef.enabled = showFinal;

		}
		// Final orbit color
		if (showFinal) {
		    //colorFinal = colorMap[icolor];
		    var cstyle = new GUIStyle();
		    var ctx = new Texture2D(1, 1);
		    cstyle.normal.background = ctx;
		    ctx.SetPixel(1,1,colorFinal);
		    ctx.Apply();
		    GUILayout.Label("Final orbit color: ");
		    GUILayout.Label(" ", cstyle);
		}
		GUILayout.EndHorizontal();

		// Final elements
		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(showFinalElements, "Show Final Elements") != showFinalElements) {
		    showFinalElements = !showFinalElements;
		}
		if (showFinalElements) {
		    GUILayout.BeginVertical();
		    GUILayout.Label("ApA: " + LengthToString(preview.orbitf.ApA));
		    GUILayout.Label("PeA: " + LengthToString(preview.orbitf.PeA));
		    GUILayout.Label("TP: " + Math.Round(preview.orbitf.period, 2) + " sec");
		    GUILayout.Label("Inc: " + Math.Round(preview.orbitf.inclination, 2) + " deg");
		    GUILayout.Label("Ecc: " + Math.Round(preview.orbitf.eccentricity, 2));
		    GUILayout.Label("LAN: " + Math.Round(preview.orbitf.LAN, 2) + " deg");
		    GUILayout.Label("AOP: " + Math.Round(preview.orbitf.argumentOfPeriapsis, 2) + " deg");
		    GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	    }
	    
	    // Target error
	    // Target line will need updating when target is deselected
	    if (FlightGlobals.fetch.VesselTarget == null) {
		updateTargetLine = true;
	    }
	    if (FlightGlobals.fetch.VesselTarget != null && showPreview && preview.orbitf != null) {
		// Calculate target line & errors if target selected & update needed
		if (FlightGlobals.fetch.VesselTarget != null && updateTargetLine) {
		    preview.CalculateTargetLine();
		    updateTargetLine = false;
		}
		// GUI to show target errors
		GUILayout.BeginHorizontal();
		if (GUILayout.Toggle(showTargetErr, "Show Target Error") != showTargetErr) {
		    showTargetErr = !showTargetErr;
		}
		// Show error between final orbit and target
		if (showTargetErr) {
		    GUILayout.BeginVertical();
		    // Distance
		    GUILayout.Label("Final distance to target: " + LengthToString(preview.targetD));
		    // Speed
		    GUILayout.Label("Final speed to target: " + SpeedToString(preview.targetV));
		    // Orbit elements
		    GUILayout.Label("Apoapsis: " + LengthToString(preview.ApErr));
		    GUILayout.Label("Periapsis: " + LengthToString(preview.PeErr));
		    GUILayout.Label("Orbital Period: " + Math.Round(preview.TPErr, 2) + " sec");
		    GUILayout.Label("Inclination: " + Math.Round(preview.IncErr, 3) + " deg");
		    GUILayout.Label("Eccentricity: " + Math.Round(preview.EccErr, 3));
		    GUILayout.Label("LAN: " + Math.Round(preview.LANErr, 3));
		    GUILayout.Label("AOP: " + Math.Round(preview.AOPErr, 3));
		    GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	    }

	    /*
	    // Show difference with target at end of control sequence
	    if (FlightGlobals.fetch.VesselTarget != null && showPreview) {
		// Distance
		GUILayout.Label("Final distance to target: " + LengthToString(preview.targetD));
		
		// Speed
		GUILayout.Label("Final speed to target: " + SpeedToString(preview.targetV));
	    }
	    */

	    GUILayout.EndVertical();

	    GUI.DragWindow();
	}

	// Controls GUI rectangle
	private Rect controlWindowPos = new Rect(0, 50, 0, 0);

	// Controls GUI function
	public void DrawControls () {
	    if (navigator.vessel == FlightGlobals.ActiveVessel)
		controlWindowPos = GUILayout.Window(10, controlWindowPos, ControlsGUI, "Controls");
	}
	
	// Add a control
	public void Add (int i) {
	    controls.Insert(i, Control.Default(navigator, this));
	    ncontrols = controls.Count;
	    colorFinal = colorMap[ncontrols % colorMap.Length];
	    Update();
	}
	
	// Remove a control
	public void Remove (int i) {
	    if (ncontrols > 1) { // Don't remove last control
		controls.RemoveAt(i);
		ncontrols = controls.Count;
		colorFinal = colorMap[ncontrols % colorMap.Length];
		Update();
	    }
	}
	
	// Return control at specified UT
	public Control Lookup (double UT) {
	    double deltaUT = UT - UT0; // Time past UT0
	    double durationSum = 0.0; // Running sum of durations
	    foreach (var control in controls) {
		// deltaUT between prior and next duration
		if (deltaUT >= durationSum && deltaUT < (durationSum += control.duration)) {
		    return control;
		}
	    }
	    // If none found, return "navigator off" control
	    return navigatorOff;
	}

	// Update the navigator's control fields
	public void Update () {
	    // Initial time
	    navigator.UT0 = UT0;
	    // Controls
	    navigator.cones = controls[0].cone.ToString();
	    navigator.clocks = controls[0].clock.ToString();
	    navigator.flatspins = controls[0].flatspin.ToString();
	    navigator.throttles = controls[0].throttle.ToString();
	    navigator.sailons = controls[0].sailon.ToString();
	    navigator.durations = controls[0].duration.ToString();
	    for (var i = 1; i < ncontrols; i++) {
		navigator.cones += delimiter + controls[i].cone.ToString();
		navigator.clocks += delimiter + controls[i].clock.ToString();
		navigator.flatspins += delimiter + controls[i].flatspin.ToString();
		navigator.throttles += delimiter + controls[i].throttle.ToString();
		navigator.sailons += delimiter + controls[i].sailon.ToString();
		navigator.durations += delimiter + controls[i].duration.ToString();
	    }
	    preview.Calculate();
	}
    }
}
