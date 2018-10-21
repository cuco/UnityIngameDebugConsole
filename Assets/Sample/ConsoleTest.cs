using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        IngameDebugConsole.DebugLogConsole.AddCommandInstance("ConsoleAdd", "instance method test", "Add", this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    [IngameDebugConsole.ConsoleMethod(RenameCommand = "ConsoleLog", Description = "static method test")]
    public static void Log(string param)
    {
        Debug.Log("ConsoleTest::Log " + param);
    }

    void Add(int a, int b)
    {
        Debug.Log(string.Format("ConsoleTest::Add {0} + {1} = {2}", a, b, a + b));
    }

    [IngameDebugConsole.ConsoleMethod]
    public static string Log2()
    {
        return "Log2";
    }

    [IngameDebugConsole.ConsoleMethod]
    public static string Log3()
    {
        return "Log3";
    }

    [IngameDebugConsole.ConsoleMethod]
    public static string Log4()
    {
        return "Log4";
    }
}
