namespace UnityEngine;

// Process-local settings surface for source compatibility. Persistent storage
// can be bound by the host without changing recovered settings decisions.
public static class PlayerPrefs {
    static readonly Dictionary<string,string> values=new(StringComparer.Ordinal);
    public static bool HasKey(string key)=>values.ContainsKey(Key(key));
    public static void DeleteKey(string key)=>values.Remove(Key(key));
    public static void DeleteAll()=>values.Clear();
    public static int GetInt(string key,int defaultValue=0)=>int.TryParse(GetString(key),out var value)?value:defaultValue;
    public static float GetFloat(string key,float defaultValue=0f)=>float.TryParse(GetString(key),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var value)?value:defaultValue;
    public static string GetString(string key,string defaultValue="")=>values.GetValueOrDefault(Key(key),defaultValue);
    public static void SetInt(string key,int value)=>SetString(key,value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    public static void SetFloat(string key,float value)=>SetString(key,value.ToString("R",System.Globalization.CultureInfo.InvariantCulture));
    public static void SetString(string key,string value)=>values[Key(key)]=value??string.Empty;
    public static void Save() { }
    static string Key(string key)=>string.IsNullOrEmpty(key)?throw new ArgumentException("Preference key is required",nameof(key)):key;
}
