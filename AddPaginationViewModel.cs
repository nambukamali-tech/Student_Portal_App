using Students_Portal_App.DTOs;

namespace Students_Portal_App.ViewModels
{
    
        public class AddPaginationViewModel
        {
            // Data to display (JOIN result)
            public List<StudentsPaperdtos> Items { get; set; } = new();

            // Pagination info
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int PageSize { get; set; }

            // Optional helpers
            public bool HasPrevious => CurrentPage > 1;
            public bool HasNext => CurrentPage < TotalPages;
        }
    }


