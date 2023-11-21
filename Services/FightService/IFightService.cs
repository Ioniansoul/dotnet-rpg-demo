namespace dotnet_rpg.Services.FightService
{
    public interface IFightService
    {
        Task<ServiceResponse<AttackResultResponseDto>> WeaponAttack(WeaponAttackDto request);
        Task<ServiceResponse<AttackResultResponseDto>> SkillAttack(SkillAttackDto request);
        Task<ServiceResponse<FightResultResponseDto>> Fight(FightRequestDto request);
        Task<ServiceResponse<List<HighScoreResponseDto>>> GetHighScore();      
        Task<ServiceResponse<List<HighScoreResponseDto>>> ResetScores();

    }
}