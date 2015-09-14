using System;
using System.Collections;
using UnityEngine;

namespace SolarSailNavigator {

    public class Utils {

	// Modulous function (instead of % operator, which is remainder)
	public static float Mod (float a, float b) {
	    return a - b * (float)Math.Floor(a / b);
	}
	
	// Normalize angles between -180 and 180 degrees
	public static float normalizeAngle (float angle) {
	    if (angle < -180 || angle > 180) {
		return Mod((angle + 180.0f), 360.0f) - 180.0f;
	    } else {
		return angle;
	    }
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

	public class Popup {
	    static int popupListHash = "PopupList".GetHashCode();
	    
	    public static bool List (Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
				     GUIStyle listStyle) {
		return List(position, ref showList, ref listEntry, buttonContent, listContent, "button", "box", listStyle);
	    }
	    
	    public static bool List (Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
				     GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle) {
		int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
		bool done = false;
		switch (Event.current.GetTypeForControl(controlID)) {
		    case EventType.mouseDown:
			if (position.Contains(Event.current.mousePosition)) {
			    GUIUtility.hotControl = controlID;
			    showList = true;
			}
			break;
		    case EventType.mouseUp:
			if (showList) {
			    done = true;
			}
			break;
		}
		
		GUI.Label(position, buttonContent, buttonStyle);
		if (showList) {
		    Rect listRect = new Rect(position.x, position.y, position.width, listStyle.CalcHeight(listContent[0], 1.0f)*listContent.Length);
		    GUI.Box(listRect, "", boxStyle);
		    listEntry = GUI.SelectionGrid(listRect, listEntry, listContent, 1, listStyle);
		}
		if (done) {
		    showList = false;
		}
		return done;
	    }
	}
    }
}
