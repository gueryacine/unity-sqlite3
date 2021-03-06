//
//  Created by Alvin Phu on 12/12/13.
//

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class SQLitePlugin : MonoBehaviour
{
	IntPtr sqliteInstance;
	
#if UNITY_EDITOR || UNITY_STANDALONE_OSX
	[DllImport("SQLitePlugin")]
	private static extern IntPtr _SQLitePlugin_Init(string gameObject);
	
	[DllImport("SQLitePlugin")]
	private static extern IntPtr _SQLitePlugin_Destroy(IntPtr instance);
	
	[DllImport("SQLitePlugin")]
	private static extern bool _SQLitePlugin_OpenDB(IntPtr instance, string dbFilename);
	
	[DllImport("SQLitePlugin")]
	private static extern void _SQLitePlugin_CloseDB(IntPtr instance);
	
	[DllImport("SQLitePlugin")]
	private static extern int _SQLitePlugin_ExecuteQuery(IntPtr instance, string queryString);
	
	[DllImport("SQLitePlugin")]
	private static extern bool _SQLitePlugin_ExecuteRowQuery(IntPtr instance, string queryString, out IntPtr outRows, out int outNumValues);
	
#elif UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern IntPtr _SQLitePlugin_Init(string gameObject);
	
	[DllImport("__Internal")]
	private static extern IntPtr _SQLitePlugin_Destroy(IntPtr instance);
	
	[DllImport("__Internal")]
	private static extern bool _SQLitePlugin_OpenDB(IntPtr instance, string dbFilename);
	
	[DllImport("__Internal")]
	private static extern void _SQLitePlugin_CloseDB(IntPtr instance);
	
	[DllImport("__Internal")]
	private static extern int _SQLitePlugin_ExecuteQuery(IntPtr instance, string queryString);
	
	[DllImport("__Internal")]
	private static extern bool _SQLitePlugin_ExecuteRowQuery(IntPtr instance, string queryString, out IntPtr outRows, out int outNumValues);
#endif
	
	public void Start()
	{
		Init();
	}
	
	public void Init()
	{
		sqliteInstance = _SQLitePlugin_Init(name);
	}

	void OnDestroy()
	{
		if (sqliteInstance == IntPtr.Zero)
			return;
		
		 _SQLitePlugin_Destroy(sqliteInstance);
	}

	public bool OpenDB(string dbFilename)
	{
		if (sqliteInstance == IntPtr.Zero)
			return false;	
		return _SQLitePlugin_OpenDB(sqliteInstance, dbFilename);
	}
	
	public void CloseDB()
	{
		if (sqliteInstance == IntPtr.Zero)
			return;
		
		_SQLitePlugin_CloseDB(sqliteInstance);
	}
	
	public int ExecuteQuery(string queryString)
	{
		if (sqliteInstance == IntPtr.Zero)
			return -1;
		
		return _SQLitePlugin_ExecuteQuery(sqliteInstance, queryString);
	}
	
	public bool ExecuteRowQuery(string queryString, out string[,] outputValues)
	{
		if (sqliteInstance == IntPtr.Zero)
		{
			outputValues = new string[0,0];
			return false;
		}
		
		IntPtr intermediateOutValues = IntPtr.Zero;
		int numOutValues = 0;
		
		// Call the Objective C plugin for the out values.
		bool success = _SQLitePlugin_ExecuteRowQuery(sqliteInstance, queryString, out intermediateOutValues, out numOutValues);
		
		outputValues = new string[0,0];
		
		// Convert the string pointer to a string that is in UTF8Format from Objective-C.
        for (int stringIndex = 0; stringIndex < numOutValues; ++stringIndex)
		{
			IntPtr stringPointer = Marshal.ReadIntPtr(intermediateOutValues, stringIndex * IntPtr.Size);		
			String autoString = Marshal.PtrToStringAuto(stringPointer);
			String[] splitStrings = autoString.Split('|');
			// Initialize the array to the correct size on the first row.
			if (outputValues.Length == 0)
			{
				outputValues = new string[numOutValues, splitStrings.Length];
			}
			
			// Split each of the columns into seperate strings as well.
			for (int splitIndex = 0; splitIndex < splitStrings.Length; splitIndex++)
			{
				outputValues[stringIndex,splitIndex] = splitStrings[splitIndex];
			}
        }
		
		return true;
	}
}