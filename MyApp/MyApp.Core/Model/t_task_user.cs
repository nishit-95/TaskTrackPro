namespace MyApp.Core.Model;

public class t_task_user
{
    public int C_Taskid { get; set; }

    public int C_Userid { get; set; }

    public string C_Title { get; set; }

    public string C_Description { get; set; }

    public int C_Estimateddays { get; set; }

    public DateOnly C_Startdate { get; set; }

    public DateOnly C_Enddate { get; set; }

    public string C_Status { get; set; }

    public string C_Document { get; set; }
}