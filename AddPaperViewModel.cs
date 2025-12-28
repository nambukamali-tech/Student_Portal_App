using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Students_Portal_App.ViewModels
{
    public class AddPaperViewModel
    {
        public required string RegisterNumber { get; set; }

        public required string PaperTitle { get; set; }
        public required string PaperDescription { get; set; }
        public required string PaperCode { get; set; }

    }
}
