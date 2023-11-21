using dotnet_rpg.Dtos.Skill;

namespace dotnet_rpg
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterResponseDto>();
            CreateMap<AddCharacterDto, Character>();   
            CreateMap<Weapon, GetWeaponResponseDto>();   
            CreateMap<Skill, GetSkillResponseDto>(); 
            CreateMap<Character, HighScoreResponseDto>();
        }
    }
}