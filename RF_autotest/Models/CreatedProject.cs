using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Models
{
    public class Programs
    {
        public IList<string> program_uuid_list { get; set; }
        public IList<string> price_group_uuid_list { get; set; }
    }

    public class DataSource
    {
        public string source { get; set; }
        public bool payable { get; set; }
    }

    public class Select
    {
        public string field { get; set; }
        public string label { get; set; }
        public string title { get; set; }
        public string function { get; set; }
        public string data_type { get; set; }
        public bool? show_by_default { get; set; }
    }

    public class AdjustmentOptions
    {
        public string bu_uuid { get; set; }
        public string product_uuid { get; set; }
        public string tier_override_column { get; set; }
        public string amount_override_column { get; set; }
    }

    public class Filter
    {
        public string field { get; set; }
        public string value { get; set; }
        public string operation { get; set; }
    }

    public class Option
    {
        public string title { get; set; }
        public IList<Select> select { get; set; }
        public string data_url { get; set; }
        public IList<string> group_by { get; set; }
        public string aggregate_url { get; set; }
        public bool is_aggregated { get; set; }
        public string columns_url { get; set; }
        public string drill_down_url { get; set; }
        public string details_data_url { get; set; }
        public AdjustmentOptions adjustment_options { get; set; }
        public string details_columns_url { get; set; }
        public IList<Filter> filter { get; set; }
    }

    public class Level
    {
        public string level { get; set; }
        public IList<Option> options { get; set; }
        public string drill_down_url { get; set; }
    }

    public class CombineDataSource
    {
        public IList<string> sources { get; set; }
        public string data_source_group_name { get; set; }
    }

    public class Field
    {
        public string field { get; set; }
        public string title { get; set; }
        public string data_type { get; set; }
    }

    public class ColumnTitleToDataUrlMap
    {
        public string data_url { get; set; }
        public string column_title { get; set; }
    }

    public class CommentPopupMetadata
    {
        public IList<Field> fields { get; set; }
        public IList<ColumnTitleToDataUrlMap> column_title_to_data_url_map { get; set; }
    }

    public class DisputeAllParameters
    {
        public string data_source { get; set; }
        public string transactions_source { get; set; }
    }

    public class DataSourceColumnBackup
    {
        public string field { get; set; }
    }

    public class DisputeCode
    {
        public string id { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public string column_name { get; set; }
    }

    public class ExclusionsPopupMetadata
    {
        public IList<DisputeCode> dispute_codes { get; set; }
    }

    public class TransactionIdColumnBackup
    {
        public string field { get; set; }
    }

    public class ReportType
    {
        public string type { get; set; }
        public string title { get; set; }
        public string group_type { get; set; }
        public IList<string> unique_key { get; set; }
        public string template_name { get; set; }
        public bool? is_required { get; set; }
    }

    public class RequiredReport
    {
        public string match { get; set; }
        public string struct_item { get; set; }
        public string require_type { get; set; }
        public string struct_title { get; set; }
    }

    public class Parameters
    {
        public IList<string> additional_fields { get; set; }
        public string project_price_groups_url { get; set; }
        public IList<Level> levels { get; set; }
        public string data_url { get; set; }
        public IList<string> show_first_titles { get; set; }
        public IList<string> optional_parameters { get; set; }
        public IList<CombineDataSource> combine_data_sources { get; set; }
        public CommentPopupMetadata comment_popup_metadata { get; set; }
        public DisputeAllParameters dispute_all_parameters { get; set; }
        public DataSourceColumnBackup data_source_column_backup { get; set; }
        public ExclusionsPopupMetadata exclusions_popup_metadata { get; set; }
        public TransactionIdColumnBackup transaction_id_column_backup { get; set; }
        public IList<ReportType> report_types { get; set; }
        public IList<RequiredReport> required_reports { get; set; }
    }

    public class CurrentSubview
    {
        public string type { get; set; }
        public string title { get; set; }
        public Parameters parameters { get; set; }
        public bool is_editable { get; set; }
        public string ui_component_url { get; set; }
        public string ui_component_name { get; set; }
        public string ui_module { get; set; }
    }

    public class Calculation
    {
        public string id { get; set; }
        public DateTime effective_date { get; set; }
    }

    public class ExecutionContextIds
    {
        public Calculation calculation { get; set; }
    }

    public class Owner
    {
        public string user { get; set; }
        public string permission { get; set; }
        public string user_last_name { get; set; }
        public string user_first_name { get; set; }
    }

    public class AdditionalAttributes
    {
        public double total_amount { get; set; }
        public double total_payment { get; set; }
    }

    public class SuccessorProjectList
    {
        public string project_uuid { get; set; }
        public string relationship_type { get; set; }
    }

    public class CreatedProject
    {
        public string id { get; set; }
        public string arrangement_uuid { get; set; }
        public IList<string> arrangement_uuid_list { get; set; }
        public string arrangement_name { get; set; }
        public string client_short_name { get; set; }
        public string client_name { get; set; }
        public string project_type { get; set; }
        public object parent_project_type { get; set; }
        public string contract_bu_id { get; set; }
        public string contract_bu_name { get; set; }
        public Programs programs { get; set; }
        public IList<DataSource> data_sources { get; set; }
        public string due_date { get; set; }
        public string calculation_frequency_unit { get; set; }
        public string calculation_frequency_value { get; set; }
        public object calendar_offset_uuid { get; set; }
        public string duration { get; set; }
        public string project_date_start { get; set; }
        public string project_date_end { get; set; }
        public string calculation_date_start { get; set; }
        public string workflow_step { get; set; }
        public IList<string> workflow_substep { get; set; }
        public IList<CurrentSubview> current_subviews { get; set; }
        public object errors_status { get; set; }
        public ExecutionContextIds execution_context_ids { get; set; }
        public object assignee { get; set; }
        public object assignee_first_name { get; set; }
        public object assignee_last_name { get; set; }
        public IList<Owner> owners { get; set; }
        public AdditionalAttributes additional_attributes { get; set; }
        public object payable_amount { get; set; }
        public object project_counter { get; set; }
        public string created_by { get; set; }
        public DateTime created_at { get; set; }
        public object changed_by { get; set; }
        public object changed_at { get; set; }
        public object approved_at { get; set; }
        public bool is_closed { get; set; }
        public object closed_at { get; set; }
        public IList<object> predecessor_project_list { get; set; }
        public IList<SuccessorProjectList> successor_project_list { get; set; }
        public IList<object> warnings { get; set; }
        public object connect_id { get; set; }
        public string project_name { get; set; }
        public string project_description { get; set; }
        public string workflow_step_title { get; set; }
        public IList<string> workflow_substep_title { get; set; }
    }

}
