using System;

[Serializable]
public class VisitorData
{
    public string name;
    public string badgeId;
    public string department;
    public string clearance;
    public string eyeColor;
    public string portraitResource;
    public bool isPseudoman;
    public bool badgeIsValid;
    public bool departmentAllowed;
    public bool clearanceAllowed;
    public bool eyeColorMatches;
}

[Serializable]
public class VisitorDatabase
{
    public VisitorData[] visitors;
}