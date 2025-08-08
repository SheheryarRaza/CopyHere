using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CopyHere.Application.DTO.Clipboard;
using CopyHere.Application.DTO.Device;
using CopyHere.Core.Entity;

namespace CopyHere.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ClipboardEntry, DTO_ClipboardEntry>()
                .ForMember(dest => dest.ContentBase64,
                           opt => opt.MapFrom(src => src.ContentBytes != null ? Convert.ToBase64String(src.ContentBytes) : null))
                .ForMember(dest => dest.Tags,
                           opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Tags) ? src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new System.Collections.Generic.List<string>()));

            // Device to DeviceDto mapping
            CreateMap<Device, DTO_Device>()
                .ForMember(dest => dest.LastSeenDescription,
                           opt => opt.MapFrom(src => GetTimeAgo(src.LastSeen)));
        }
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays} days ago";
            return $"on {dateTime:MMM dd, yyyy}";
        }
    }
}
