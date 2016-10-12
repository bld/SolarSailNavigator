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
	// Angles
	public float [] angles;
	public string [] anglestr;
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
	// Windows
	public FrameWindow frameWindow;

	// Time Conversion
	public static double SecondsPerDay = 21600.0;
	public static double SecondsPerHour = 3600.0;
	public static double SecondsPerMinute = 60.0;
	public static double HoursPerDay = 6.0;
	public static double[] warpLevels = { 1, 2, 3, 4, 5, 10, 50, 100, 1000, 10000, 100000 };
	
	// Angle controls
	public void GUIAngle (ref float angle, ref string angle_str, string name) {
	    // Text field
	    GUILayout.BeginVertical();
	    // Name of angle over top
	    GUILayout.Label(name, GUILayout.Width(80));
	    GUILayout.BeginHorizontal();
	    // Text field & buttons below
	    string new_str = GUILayout.TextField(angle_str, GUILayout.Width(30));
	    if (new_str != angle_str) {
		angle_str = new_str;
		float parsedValue;
		if (Single.TryParse(angle_str, out parsedValue)) {
		    angle = Utils.normalizeAngle(parsedValue);
		    controls.Update();
		}
	    }
	    // +/- buttons
	    if (GUILayout.Button("+")) {
		angle = Utils.normalizeAngle(angle + 5);
		angle_str = angle.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		angle = Utils.normalizeAngle(angle - 5);
		angle_str = angle.ToString();
		controls.Update();
	    }
	    // End GUI
	    GUILayout.EndHorizontal();
	    GUILayout.EndVertical();
	}

	// Throttle controls
	public void GUIThrottle () {
	    GUILayout.BeginVertical();

	    // Throttle control
	    // Text field
	    GUILayout.BeginHorizontal();
	    string new_str = GUILayout.TextField(throttle_str, GUILayout.Width(30));
	    if (new_str != throttle_str) {
		throttle_str = new_str;
		float parsedValue;
		if (Single.TryParse(throttle_str, out parsedValue)) {
		    if (parsedValue >= 0 && parsedValue <= 1) {
			throttle = parsedValue;
			controls.Update();
		    }
		}
	    }
	    // +/- buttons
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
	    // Text field
	    string new_days_str = GUILayout.TextField(days_str, GUILayout.Width(30));
	    if (new_days_str != days_str) {
		days_str = new_days_str;
		int parsedDays;
		if (Int32.TryParse(days_str, out parsedDays)) {
		    if (parsedDays >= 0) {
			days = (double)parsedDays;
			duration = SecondsPerDay * days + SecondsPerHour * hours;
			controls.Update();
		    }
		}
	    }
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
	    // Text field
	    string new_hours_str = GUILayout.TextField(hours_str, GUILayout.Width(30));
	    if (new_hours_str != hours_str) {
		hours_str = new_hours_str;
		int parsedHours;
		if (Int32.TryParse(hours_str, out parsedHours)) {
		    if (parsedHours >= 0 && parsedHours <= HoursPerDay) {
			hours = (double)parsedHours;
			duration = SecondsPerDay * days + SecondsPerHour * hours;
			controls.Update();
		    }
		}
	    }
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
	    GUILayout.Label(" ", cstyle, GUILayout.Width(30), GUILayout.Height(30));
	}

	public void GUIFrame () {
	    if (GUILayout.Button(frame.name, GUILayout.Width(40))) {
		frameWindow.show = true;
		// Run Controls.Update when window returns

	    }
	}
	
	// GUI line
	public void GUILine (Color color, int i) {
	    // Vertical space
	    GUILayout.Space(5);

	    GUILayout.BeginHorizontal();
	    
	    GUIAngle(ref angles[0], ref anglestr[0], frame.names[0]);
	    GUIAngle(ref angles[1], ref anglestr[1], frame.names[1]);
	    GUIAngle(ref angles[2], ref anglestr[2], frame.names[2]);
	    GUIThrottle();
	    GUITime();
	    GUIColor(color);
	    GUIFrame();

	    // Add/Remove buttons
	    if (GUILayout.Button("INS")) { controls.Add(i); };
	    if (GUILayout.Button("DEL")) { controls.Remove(i); };
 
	    GUILayout.EndHorizontal();

	    // Vertical space
	    GUILayout.Space(5);
	}
	
	// Constructor

	public Control (Navigator navigator, Controls controls, float [] angles, float throttle, bool sailon, double duration, int iwarp, string frame) {
	    // Navigator
	    this.navigator = navigator;
	    // Parent controls object
	    this.controls = controls;
	    // Reference frame for control angles
	    this.frame = Frame.Frames[frame];
	    // Frame window
	    frameWindow = new FrameWindow(this);
	    // Angles
	    this.angles = angles;
	    // Angle strings
	    anglestr = new string [3];
	    for (int i = 0; i < 3; i++) {
		anglestr[i] = angles[i].ToString();
	    }
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
	    var frame = Frame.Frames[navigator.defaultFrame];
	    var defaultAngles = new float [] { navigator.defaultAngle0, navigator.defaultAngle1, navigator.defaultAngle2 };
	    var defaultDuration = navigator.defaultDays * SecondsPerDay + navigator.defaultHours * SecondsPerHour;
	    return new Control(navigator, controls, defaultAngles, navigator.defaultThrottle, navigator.defaultSailon, defaultDuration, navigator.defaultIWarp, navigator.defaultFrame);
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
	int id; // GUI window id
	
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

	// Defaults window
	public DefaultWindow defaultWindow;
	
	// Constructor

	// Give the navigator to which this control is for
	public Controls (Navigator navigator) {

	    // Defaults window
	    defaultWindow = new DefaultWindow(navigator);
	    
	    // Assign navigator field
	    this.navigator = navigator;
	    Debug.Log(this.navigator.ToString());

	    // Controls window id
	    id = GUIUtility.GetControlID(FocusType.Keyboard);

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
	    if (String.IsNullOrEmpty(navigator.frames) ||
		String.IsNullOrEmpty(navigator.angle0s) ||
		String.IsNullOrEmpty(navigator.angle1s) ||
		String.IsNullOrEmpty(navigator.angle2s) ||
		String.IsNullOrEmpty(navigator.throttles) ||
		String.IsNullOrEmpty(navigator.sailons) ||
		String.IsNullOrEmpty(navigator.durations)) {
		ncontrols = 1;
		controls = new List<Control>();
		controls.Add(Control.Default(navigator, this));

	    } else { // Otherwise, parse saved controls

		// Split into arrays
		var frameStrings = navigator.frames.Split(delimiter);
		var angle0Strings = navigator.angle0s.Split(delimiter);
		var angle1Strings = navigator.angle1s.Split(delimiter);
		var angle2Strings = navigator.angle2s.Split(delimiter);
		var throttleStrings = navigator.throttles.Split(delimiter);
		var durationStrings = navigator.durations.Split(delimiter);
		var sailonStrings = navigator.sailons.Split(delimiter);

		// Find number of controls
		ncontrols =
		    Math.Min(frameStrings.Length,
			     Math.Min(angle0Strings.Length,
				      Math.Min(angle1Strings.Length,
					       Math.Min(angle2Strings.Length,
							Math.Min(throttleStrings.Length,
								 Math.Min(durationStrings.Length,
									  sailonStrings.Length))))));
		
		// Initialize controls array
		controls = new List<Control>();

		// Populate controls
		for(var i = 0; i < ncontrols; i++) {
		    var angles = new float [] { Utils.ParseSingle(angle0Strings[i]),
						Utils.ParseSingle(angle1Strings[i]),
						Utils.ParseSingle(angle2Strings[i]) };
		    controls.Add(new Control(navigator,
					     this,
					     angles,
					     Utils.ParseSingle(throttleStrings[i]),
					     Utils.ParseBool(sailonStrings[i]),
					     Utils.ParseDouble(durationStrings[i]),
					     navigator.defaultIWarp,
					     frameStrings[i]));
		}
	    }

	    // Preview
	    preview = new Preview(navigator);
	}

	// Convert length in meters to string with bigger units
	string LengthToString (double lengthm) {
	    if (Math.Abs(lengthm) < 1000) {
		return Math.Round(lengthm).ToString() + " m";
	    } else if (Math.Abs(lengthm) < 1000000000) {
		return Math.Round(lengthm / 1000, 3).ToString() + " km";
	    } else {
		return Math.Round(lengthm / 1000000000, 4).ToString() + " Gm";
	    }
	}

	// Convert speed in m/s to string with bigger units
	string SpeedToString (double speedms) {
	    if (Math.Abs(speedms) < 1000) {
		return Math.Round(speedms, 1).ToString() + " m/s";
	    } else {
		return Math.Round(speedms / 1000.0, 3).ToString() + " km/s";
	    }
	}
	
	
	// GUI
	public void ControlsGUI (int WindowID) {
	    GUILayout.BeginVertical();

	    // Lock/Unlock attitude
	    navigator.IsLocked = GUILayout.Toggle(navigator.IsLocked, "Lock Attitude");

	    // Check for any other locked navigators
	    if (navigator.IsLocked) {
		foreach (var p in navigator.vessel.parts) {
		    foreach (var m in p.Modules) {
			if (!System.Object.ReferenceEquals(m, navigator) && m is Navigator && (m as Navigator).IsLocked) {
			    navigator.IsLocked = false;
			    Debug.Log("[SolarSailNavigator] " + (m as PartModule).part.partName + " is already locked. Cannot lock " + navigator.part.partName);
			}
		    }
		}
	    }

	    // Set the initial time of the sequence
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Start time: " + UT0.ToString());
	    if(GUILayout.Button("Set to Now")) {
		UT0 = Planetarium.GetUniversalTime();
		Update();
	    }
	    GUILayout.EndHorizontal();

	    // Defaults
	    defaultWindow.show = GUILayout.Toggle(defaultWindow.show, "Set Defaults");

	    // Controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Angle 1", GUILayout.Width(80));
	    GUILayout.Label("Angle 2", GUILayout.Width(80));
	    GUILayout.Label("Angle 3", GUILayout.Width(80));
	    GUILayout.Label("Throttle", GUILayout.Width(80));
	    GUILayout.Label("Days", GUILayout.Width(115));
	    GUILayout.Label("Hours", GUILayout.Width(70));
	    GUILayout.Label("Color", GUILayout.Width(30));
	    GUILayout.Label("Frame", GUILayout.Width(40));
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
		controlWindowPos = GUILayout.Window(id, controlWindowPos, ControlsGUI, "Controls");
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
	    navigator.frames = controls[0].frame.name;
	    navigator.angle0s = controls[0].angles[0].ToString();
	    navigator.angle1s = controls[0].angles[1].ToString();
	    navigator.angle2s = controls[0].angles[2].ToString();
	    navigator.throttles = controls[0].throttle.ToString();
	    navigator.sailons = controls[0].sailon.ToString();
	    navigator.durations = controls[0].duration.ToString();
	    for (var i = 1; i < ncontrols; i++) {
		navigator.frames += delimiter + controls[i].frame.name;
		navigator.angle0s += delimiter + controls[i].angles[0].ToString();
		navigator.angle1s += delimiter + controls[i].angles[1].ToString();
		navigator.angle2s += delimiter + controls[i].angles[2].ToString();
		navigator.throttles += delimiter + controls[i].throttle.ToString();
		navigator.sailons += delimiter + controls[i].sailon.ToString();
		navigator.durations += delimiter + controls[i].duration.ToString();
	    }
	    preview.Calculate();
	}
    }
}
