using dotnet_rpg.Dtos.Skill;

namespace dotnet_rpg.Dtos.Character
{
    public class GetCharacterResponseDto
    {      
        public int Id { get; set; }
        public string Name { get; set; } = "Frodo";
        public int HitPoints { get; set; } = 100;
        public int Strength { get; set; } = 10;
        public int Agility { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public RpgClass Class { get; set; } = RpgClass.Warrior;     
        public GetWeaponResponseDto? Weapon { get; set; }    
        public List<GetSkillResponseDto>? Skills { get; set; }
        public int Fights { get; set; }
        public int Victories { get; set; }
        
        public int Defeats { get; set; }   
    }
}