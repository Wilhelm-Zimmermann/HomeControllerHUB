using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Domain.Mappings;

public class IMapFrom<T>
{
    public void Mapping(Profile profile)
    {
        var sourceType = typeof(T);
        var destinationType = GetType();

        var mappingExpression = profile.CreateMap(sourceType, destinationType);

        foreach (var property in destinationType.GetProperties())
        {
            var mapFromAttribute = property.GetCustomAttribute<MapFromAttribute>();
            if (sourceType.GetProperty(property.Name) == null)
            {
                mappingExpression.ForMember(property.Name, opt => opt.Ignore());
            }
            if (mapFromAttribute != null && sourceType.GetProperty(mapFromAttribute.Name) != null)
            {
                mappingExpression.ForMember(property.Name, opt =>
                {
                    if (property.PropertyType == typeof(bool)) opt.MapFrom(src => EF.Property<bool>(src, mapFromAttribute.Name));
                    else if (property.PropertyType == typeof(int)) opt.MapFrom(src => EF.Property<int>(src, mapFromAttribute.Name));
                    else opt.MapFrom(src => EF.Property<string>(src, mapFromAttribute.Name));
                });
            }
        }
        mappingExpression.ReverseMap();
    }
}