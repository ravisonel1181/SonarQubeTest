using System.Collections.Generic;

namespace NEHGSWebApiCore.Application.DTOs
{
    public class MapRequestDto
    {
        public string ParentDirectory { get; set; }
        public int SelectedCollectionId { get; set; }
        public int SelectedRecordTypeId { get; set; }
        public string ImportName { get; set; }
        public Dictionary<string, ColumnMapDto> ColumnNameToMapping { get; set; }
        public bool IsDuplicateChk { get; set; }
    }

    public class ColumnMapDto
    {
        public string MappedType { get; set; }
        public string MappedField { get; set; }
        public string MappedRole { get; set; }
    }

    public class PreviewResponseDto
    {
        // For simplicity preview is represented as array of rows where each row is column->value map
        public IEnumerable<Dictionary<string, object>> Rows { get; set; }
    }

    public class ErrorCheckRequestDto
    {
        public string ParentDirectory { get; set; }
        public Dictionary<string, ColumnMapDto> ColumnNameToMapping { get; set; }
    }

    public class ConfirmMapResultDto
    {
        public bool IsMultipleCSVs { get; set; }
        public string MissingColumnsDetails { get; set; }
        public string MismatchColumnDiff { get; set; }
    }

    public class ProgressResponseDto
    {
        public int Percent { get; set; }
        public string TempListJson { get; set; }
    }
}
