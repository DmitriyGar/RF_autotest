using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models
{
    public class CreatedProject
    { 
        public string id { get; set; }
        public string arrangement_uuid { get; set; }
        public string[] arrangement_uuid_list { get; set; }
        public string arrangement_name { get; set; }
        public string client_short_name { get; set; }
        public string client_name { get; set; }
        public string project_type { get; set; }
        public object parent_project_type { get; set; }
        public string contract_bu_id { get; set; }
        public string contract_bu_name { get; set; }
        public Programs programs { get; set; }
        public Data_Sources[] data_sources { get; set; }
        public string due_date { get; set; }
        public string calculation_frequency_unit { get; set; }
        public int calculation_frequency_value { get; set; }
        public object calendar_offset_uuid { get; set; }
        public int duration { get; set; }
        public string project_date_start { get; set; }
        public string project_date_end { get; set; }
        public string calculation_date_start { get; set; }
        public string workflow_step { get; set; }
        public string[] workflow_substep { get; set; }
        public Current_Subviews[] current_subviews { get; set; }
        public object errors_status { get; set; }
        public object execution_context_ids { get; set; }
        public object assignee { get; set; }
        public object assignee_first_name { get; set; }
        public object assignee_last_name { get; set; }
        public IList<Owner> owners { get; set; }
        public Additional_Attributes additional_attributes { get; set; }
        public object payable_amount { get; set; }
        public object project_counter { get; set; }
        public string created_by { get; set; }
        public DateTime created_at { get; set; }
        public object changed_by { get; set; }
        public object changed_at { get; set; }
        public object approved_at { get; set; }
        public bool is_closed { get; set; }
        public object closed_at { get; set; }
        public object predecessor_project_list { get; set; }
        public object successor_project_list { get; set; }
        public object warnings { get; set; }
        public object connect_id { get; set; }
        public string project_name { get; set; }
        public string project_description { get; set; }
        public string project_type_title { get; set; }
    }
    public class Owner
    {
        public string user { get; set; }
        public string permission { get; set; }
        public string user_last_name { get; set; }
        public string user_first_name { get; set; }
    }
    public class Additional_Attributes
    {
        public string total_amount { get; set; }
        public string total_payment { get; set; }
    }

    public class Data_Sources
    {
        public string source { get; set; }
        public bool payable { get; set; }
    }

    public class Current_Subviews
    {
        public string type { get; set; }
        public string title { get; set; }
        public Parameters parameters { get; set; }
        public bool is_editable { get; set; }
        public string ui_component_url { get; set; }
        public string ui_component_name { get; set; }
    }

    public class Parameters
    {
        public string[] additional_fields { get; set; }
        public string project_price_groups_url { get; set; }
        public Level[] levels { get; set; }
        public string data_url { get; set; }
        public string[] show_first_titles { get; set; }
        public string[] optional_parameters { get; set; }
        public Combine_Data_Sources[] combine_data_sources { get; set; }
        public Comment_Popup_Metadata comment_popup_metadata { get; set; }
        public Dispute_All_Parameters dispute_all_parameters { get; set; }
        public Data_Source_Column_Backup data_source_column_backup { get; set; }
        public Exclusions_Popup_Metadata exclusions_popup_metadata { get; set; }
        public Transaction_Id_Column_Backup transaction_id_column_backup { get; set; }
        public Report_Types[] report_types { get; set; }
        public Required_Reports[] required_reports { get; set; }
    }

    public class Comment_Popup_Metadata
    {
        public Field[] fields { get; set; }
        public Column_Title_To_Data_Url_Map[] column_title_to_data_url_map { get; set; }
    }

    public class Field
    {
        public string field { get; set; }
        public string title { get; set; }
        public string data_type { get; set; }
    }

    public class Column_Title_To_Data_Url_Map
    {
        public string data_url { get; set; }
        public string column_title { get; set; }
    }

    public class Dispute_All_Parameters
    {
        public string data_source { get; set; }
        public string transactions_source { get; set; }
    }

    public class Data_Source_Column_Backup
    {
        public string field { get; set; }
    }

    public class Exclusions_Popup_Metadata
    {
        public Dispute_Codes[] dispute_codes { get; set; }
    }

    public class Dispute_Codes
    {
        public string id { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public string column_name { get; set; }
    }

    public class Transaction_Id_Column_Backup
    {
        public string field { get; set; }
    }

    public class Level
    {
        public string level { get; set; }
        public Option[] options { get; set; }
        public string drill_down_url { get; set; }
    }

    public class Option
    {
        public string title { get; set; }
        public Select[] select { get; set; }
        public string data_url { get; set; }
        public string[] group_by { get; set; }
        public string aggregate_url { get; set; }
        public bool is_aggregated { get; set; }
        public Filter[] filter { get; set; }
        public bool default_for_partial_setup { get; set; }
        public string columns_url { get; set; }
        public string drill_down_url { get; set; }
        public string details_data_url { get; set; }
        public Adjustment_Options adjustment_options { get; set; }
        public string details_columns_url { get; set; }
    }

    public class Adjustment_Options
    {
        public string bu_uuid { get; set; }
        public string product_uuid { get; set; }
        public string tier_override_column { get; set; }
        public string amount_override_column { get; set; }
    }

    public class Select
    {
        public string field { get; set; }
        public string label { get; set; }
        public string title { get; set; }
        public string function { get; set; }
        public string data_type { get; set; }
        public bool show_by_default { get; set; }
    }

    public class Filter
    {
        public string field { get; set; }
        public string value { get; set; }
        public string operation { get; set; }
    }

    public class Combine_Data_Sources
    {
        public string[] sources { get; set; }
        public string data_source_group_name { get; set; }
    }

    public class Report_Types
    {
        public string type { get; set; }
        public string title { get; set; }
        public string group_type { get; set; }
        public string[] unique_key { get; set; }
        public string template_name { get; set; }
        public string group_type_title { get; set; }
        public bool is_required { get; set; }
    }

    public class Required_Reports
    {
        public string match { get; set; }
        public string struct_item { get; set; }
        public string require_type { get; set; }
        public string struct_title { get; set; }
    }

}
