Scriptname _SChSpellChargingRead extends ObjectReference  
Spell Property _SChSpellChargingAbility Auto
Spell Property _SChSpellChargingAbilityv2 Auto

Actor Property player Auto
Float Property playerScale Auto

Event OnRead()
	player = Game.GetPlayer()
	playerScale = player.GetScale()
	;Game.GetPlayer().AddSpell(_SChSpellChargingAbility)
	;player.SetScale(playerScale)
	
	;Utility.Wait(0.5)
	if(player.HasSpell(_SChSpellChargingAbility))
		player.RemoveSpell(_SChSpellChargingAbility)
	endif
	if(player.HasSpell(_SChSpellChargingAbilityv2))
		player.RemoveSpell(_SChSpellChargingAbilityv2)
	endif
	player.AddSpell(_SChSpellChargingAbilityv2)
	Debug.Trace("SCh-Update: Done!")
EndEvent
