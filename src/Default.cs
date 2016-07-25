using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolarSailNavigator {

    public class DefaultWindow {
	// Rectangle object
	Rect windowPos;
	int frameID; // Window ID
	public bool show = false;
	Navigator navigator;
	Dictionary<string, string> fields;

	public DefaultWindow (Navigator navigator) {
	    this.navigator = navigator;
	    windowPos = new Rect(705, 100, 0, 0);
	    frameID = GUIUtility.GetControlID(FocusType.Keyboard);
	    fields = new Dictionary<string, string>();
	    fields["Frame"] = navigator.defaultFrame;
	    fields["Angle(0)"] = navigator.defaultAngle0.ToString();
	    fields["Angle(1)"] = navigator.defaultAngle1.ToString();
	    fields["Angle(2)"] = navigator.defaultAngle2.ToString();
	    fields["Days"] = navigator.defaultDays.ToString();
	    fields["Hours"] = navigator.defaultHours.ToString();
	    fields["Throttle"] = navigator.defaultThrottle.ToString();
	    fields["Sail on"] = navigator.defaultSailon.ToString();
	}

	public void DrawWindow () {
	    if (show) {
		windowPos = GUILayout.Window(frameID, windowPos, WindowGUI, "Defaults");
	    }
	}

	void WindowGUI (int WindowID) {
	    GUILayout.BeginVertical();
	    foreach(var key in fields.Keys) {
		GUILayout.BeginHorizontal();
		GUILayout.Label(key, GUILayout.Width(80));
		var newstr = GUILayout.TextField(fields[key], GUILayout.Width(80));
		float parsedSingle;
		int parsedInt;
		if (newstr != fields[key]) {
		    fields[key] = newstr;
		    switch (key) {
			case "Frame":
			    if (Frame.Frames.ContainsKey(newstr)) {
				navigator.defaultFrame = newstr;
			    }
			    break;
			case "Angle(0)":
			    if (Single.TryParse(fields[key], out parsedSingle)) {
				navigator.defaultAngle0 = Utils.normalizeAngle(parsedSingle);
			    }
			    break;
			case "Angle(1)":
			    if (Single.TryParse(fields[key], out parsedSingle)) {
				navigator.defaultAngle1 = Utils.normalizeAngle(parsedSingle);
			    }
			    break;
			case "Angle(2)":
			    if (Single.TryParse(fields[key], out parsedSingle)) {
				navigator.defaultAngle2 = Utils.normalizeAngle(parsedSingle);
			    }
			    break;
			case "Days":
			    if (Int32.TryParse(fields[key], out parsedInt)) {
				if (parsedInt >= 0) {
				    navigator.defaultDays = (double)parsedInt;
				}
			    }
			    break;
			case "Hours":
			    if (Int32.TryParse(fields[key], out parsedInt)) {
				if (parsedInt >= 0 && parsedInt <= 6) {
				    navigator.defaultHours = (double)parsedInt;
				}
			    }
			    break;
			    
		    }
		}
		
		GUILayout.Label(fields[key], GUILayout.Width(80));
		GUILayout.EndHorizontal();
	    }
	    GUILayout.EndVertical();
	    GUI.DragWindow();
	}
    }
}

	