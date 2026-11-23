using System.ComponentModel.DataAnnotations;
using InterCase.Common.Helpers;

namespace InterCase.Common.Dtos.Paging
{
    public sealed record PageRequest
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        [Range(1, int.MaxValue, ErrorMessage = Localization.Keys.PageInvalid)]
        public int Page { get; init; } = 1;

        [Range(1, MaxPageSize, ErrorMessage = Localization.Keys.PageSizeInvalid)]
        public int PageSize { get; init; } = DefaultPageSize;
    }
}
