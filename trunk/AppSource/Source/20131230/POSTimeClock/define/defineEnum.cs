#region history

// defineEnum is created to define all enum in program. 
//
// Name: Arron
// Date: 8/14/2012 original created
// 
// Add: 8/14/2012 by ...
//           Add ..
//
// Modified: 3/24/2012 by ...
//           Fixed ...

#endregion

namespace CPC.TimeClock
{

    public enum HolidayOption
    {
        SpecificDay = 0,
        DynamicDay = 1,
        Duration = 2
    }

    // [Flags]
    public enum WorkPermissionTypes
    {
        None = 0,
        ArrivingLate = 1,
        LeavingEarly = 2,
        SwitchHours = 4,
        Absence = 8,
        SickLeave = 16,
        Vacations = 32,
        DisciplinaryLeave = 64,
        BeforeClockInHour = 128,
        BreakTime = 256,
        AfterClockOutHour = 512,
        HolidayOrDayOff = 1024,
        All = ArrivingLate | LeavingEarly | SwitchHours | Absence | SickLeave | Vacations | DisciplinaryLeave | BeforeClockInHour | AfterClockOutHour | BreakTime | HolidayOrDayOff
    }

    public enum PermissionTypes
    {
        None = 0,
        Read = 1, // View
        Write = 2, // Save
        Modify = 4, // Update
        Delete = 8, // Remove
        Create = 16, // New
        Print = 32,
        Export = 64, // Excel, Word
        All = Read | Write | Modify | Delete | Create | Print | Export
    }

    public enum FunctionTypes
    {
        None = 0,
        Employee = 1,
        Customer = 2
    }

    public enum SearchType
    {
        None = 0,
        Text = 1,
        Status = 2,
        Numeric = 4,
        Currency = 8,
        Percent = 16,
        Date = 32
    }

    public enum Overtime
    {
        None = 0,
        Before = 1,
        Break = 2,
        After = 4,
        Holiday = 8
    }

    public enum SelectionTypes
    {
        None = 0,
        Selected = 1
    }

    public enum ItemTypes
    {
        None = 0,
        Inventory = 1,
        NonInventory = 2,
        Services = 3,
        Insurance = 4,
        Customize = 5,
        ComboDeal = 6
    }

    public enum ProductCommissionTypes
    {
        None = 0,
        Percent = 1,
        Money = 2
    }

    public enum LengthOfInsuranceTypes
    {
        None = 0,
        Days = 1,
        Weeks = 2,
        Months = 3,
        Years = 4
    }

    public enum WarrantyTypes
    {
        None = 0,
        Manufactory = 1,
        Extention = 2
    }

    public enum WarrantyPeriodTypes
    {
        None = 0,
        Days = 1,
        Weeks = 2,
        Months = 3,
        Years = 4
    }

    public enum ScheduleTypes
    {
        Fixed = 1,
        Variables = 2,
        Rotate = 3
    }

    public enum ProductStatuses
    {
        Inactive = 0,
        Active = 1
    }

    public enum ScheduleStatuses
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }

    public enum EmployeeScheduleStatuses
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }

    public enum FingerprintOptions
    {
        Never = 1,
        Sensor = 2,
        All = 3,
        Computer = 4
    }
}