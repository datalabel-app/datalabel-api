namespace DataLabeling.API.DTOs
{
    public class CreateAnnotationDto
    {
        public int TaskId { get; set; }

        public int LabelId { get; set; }

        public int RoundId { get; set; }

        public string Classification { get; set; } = "";

        public string ShapeType { get; set; } = "classification";

        public string Coordinates { get; set; } = "";
    }
}
