namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public FightService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<FightResultResponseDto>> Fight(FightRequestDto request)
        {
            var serviceResponse = new ServiceResponse<FightResultResponseDto>
            {
                Data = new FightResultResponseDto()
            };

            try
            {
                var characters = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .Where(c => request.CharacterIds.Contains(c.Id))
                    .ToListAsync();

                bool defeated = false;

                if(characters.Count < 2)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Insufficient character(s) to fight.";
                    return serviceResponse;
                }

                while(!defeated)
                {
                    foreach(var attacker in characters)
                    {
                        var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;
                        if(useWeapon && attacker.Weapon is not null)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);
                        }
                        else if(!useWeapon && attacker.Skills is not null)
                        {
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill);
                        }
                        else
                        {
                            serviceResponse.Data.Log
                                .Add($"{attacker.Name} wasn't able to attack!");
                            continue;
                        }
                        
                        serviceResponse.Data.Log
                            .Add($"{attacker.Name} attacked {opponent.Name} using {attackUsed} for {(damage >= 0 ? damage : 0)} damage. {opponent.Name} has {(opponent.HitPoints >= 0 ? opponent.HitPoints : 0)}/500 health remaining.");                      

                        if(opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            serviceResponse.Data.Log
                                .Add($"{opponent.Name} has been defeated!");
                            serviceResponse.Data.Log
                                .Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                            break;
                        }                 
                    }
                }
                characters.ForEach(c => 
                {
                    c.Fights++;
                    c.HitPoints = 500;
                });

                await _context.SaveChangesAsync();
            }

            catch (Exception ex)                      
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<AttackResultResponseDto>> SkillAttack(SkillAttackDto request)
      {
            var serviceResponse = new ServiceResponse<AttackResultResponseDto>();

            try
            {
                var attacker =
                    await _context.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent =
                    await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker is null || opponent is null || attacker.Skills is null)
                {
                    throw new Exception("Something fishy is going on here..");
                }

                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);
                if (skill is null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"{attacker.Name} doesn't know that skill.";
                    return serviceResponse;
                }

                int damage = DoSkillAttack(attacker, opponent, skill);

                if (opponent.HitPoints <= 0)
                    serviceResponse.Message = $"{opponent.Name} has been defeated!";

                await _context.SaveChangesAsync();

                serviceResponse.Data = new AttackResultResponseDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHp = attacker.HitPoints,
                    OpponentHp = opponent.HitPoints,
                    Damage = damage
                };
            }


            catch (Exception ex)                      
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            
            return serviceResponse;
        }

        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            int damage = skill.Damage + (new Random().Next(attacker.Intelligence));

            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<AttackResultResponseDto>> WeaponAttack(WeaponAttackDto request)
        {
            var serviceResponse = new ServiceResponse<AttackResultResponseDto>();

            try
            {
                var attacker =
                    await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent =
                    await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker is null || opponent is null)
                {
                    throw new Exception("Something fishy is going on here..");
                }
                int damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HitPoints <= 0)
                    serviceResponse.Message = $"{opponent.Name} has been defeated!";

                await _context.SaveChangesAsync();

                serviceResponse.Data = new AttackResultResponseDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHp = attacker.HitPoints,
                    OpponentHp = opponent.HitPoints,
                    Damage = damage
                };
            }

            catch (Exception ex)                      
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            
            return serviceResponse;
        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            if(attacker.Weapon is null)
                throw new Exception("Attacker has no weapon.");

            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));

            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<List<HighScoreResponseDto>>> GetHighScore()
        {
            var serviceResponse = new ServiceResponse<List<HighScoreResponseDto>>();
            var characters = await _context.Characters
                .Where(c => c.Fights > 0)
                .OrderByDescending(c => c.Victories)
                .ThenBy(c => c.Defeats)
                .ToListAsync();

            serviceResponse.Data = characters.Select(c => _mapper.Map<HighScoreResponseDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<HighScoreResponseDto>>> ResetScores()
        {
            var serviceResponse = new ServiceResponse<List<HighScoreResponseDto>>();
            var characters = await _context.Characters
                .ToListAsync();
            if(characters is null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "No characters were found.";
                return serviceResponse;
            }
            foreach (var character in characters)
            {
                character.Defeats = 0;
                character.Victories = 0;
                character.Fights = 0;
            }

            await _context.SaveChangesAsync();
            serviceResponse.Data = characters.Select(c => _mapper.Map<HighScoreResponseDto>(c)).ToList();
            return serviceResponse;            
        }
    }
}