namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;

        public FightController(IFightService fightService)
        {
            _fightService = fightService;
        }

        [HttpPost("WeaponAttack")]
        public async Task<ActionResult<ServiceResponse<AttackResultResponseDto>>> WeaponAttack(WeaponAttackDto request)
        {
            return Ok(await _fightService.WeaponAttack(request));
        }

        [HttpPost("SkillAttack")]
        public async Task<ActionResult<ServiceResponse<AttackResultResponseDto>>> SkillAttack(SkillAttackDto request)
        {
            return Ok(await _fightService.SkillAttack(request));
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse<FightResultResponseDto>>> Fight(FightRequestDto request)
        {
            return Ok(await _fightService.Fight(request));
        }

        [HttpGet("HighScores")]
        public async Task<ActionResult<ServiceResponse<List<HighScoreResponseDto>>>> GetHighScore()
        {
            return Ok(await _fightService.GetHighScore());
        }

        [HttpDelete("ResetScores")]
        public async Task<ActionResult<ServiceResponse<List<HighScoreResponseDto>>>> ResetScores()
        {
            return Ok(await _fightService.ResetScores());
        }
    }
}