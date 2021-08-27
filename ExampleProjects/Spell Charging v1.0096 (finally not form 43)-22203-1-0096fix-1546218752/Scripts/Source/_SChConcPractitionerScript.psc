Scriptname _SChConcPractitionerScript extends activemagiceffect  

GlobalVariable Property _SCChargeLevelGlobal Auto
GlobalVariable Property _SCChargeElementID Auto
GlobalVariable Property _SChIsEnabledGlobal Auto
GlobalVariable Property _SChCanChargeConcGlobal Auto
GlobalVariable Property _SChChargeSizeGlobal Auto
GlobalVariable Property _SChIsChargingGlobal Auto

Actor Property wiseman Auto
Float Property wisemanSpeed Auto

Event OnEffectStart(Actor akTarget, Actor akCaster)
	wiseman = Game.GetPlayer()
	wisemanSpeed = 1
	;*****Declare the sizes of the arrays immediately*****
	_SChMagni = new Float[25]
	_SChDur = new Int[25]
	_SChArea = new Int[25]
	_SChMagEffTrace = new MagicEffect[25]
	
	GoToState("preCharge")
EndEvent

Int Property _SChKey Auto
Int Property _SChKeyGamePad Auto
bool Property usesGamePad Auto
String Property _SChMagicSkill Auto
Spell Property _SChSpell Auto
MagicEffect Property _SChMagEff Auto
;*****_SChMagEffTrace is for tracing purposes*****
MagicEffect[] Property _SChMagEffTrace Auto
bool Property chargingFlag Auto
Float[] Property _SChMagni Auto
Int[] Property _SChDur Auto
Int[] Property _SChArea Auto
Int Property loopCounter Auto

;*****This function gets the original stats of the spell*****
Function getOriginalSpellStatsConc()
	Debug.Trace("_SChpConc: Getting original spell stats!")
	loopCounter = 0
	;*****Discovery! Even though the array size is set, dynamic arrays can be faked
	;by setting the loop condition as the end of the dynamic value*****
	while loopCounter < _SChSpell.GetNumEffects()
		_SChMagEffTrace[loopCounter] = _SChSpell.GetNthEffectMagicEffect(loopCounter)
		if _SChMagEffTrace[loopCounter] != None
			_SChMagni[loopCounter] = _SChSpell.GetNthEffectMagnitude(loopCounter)
			_SChDur[loopCounter] = _SChSpell.GetNthEffectDuration(loopCounter)
			_SChArea[loopCounter] = _SChSpell.GetNthEffectArea(loopCounter)
			Debug.Trace("_SChpConc: Index "+loopCounter+" Magic Effect "\
			+_SChMagEffTrace[loopCounter].GetName()+" has magnitude of "\
			+_SChMagni[loopCounter]+" with area of "\
			+_SChArea[loopCounter]+" and duration of "+_SChDur[loopCounter])
		endif
		loopCounter += 1
	endwhile
EndFunction

;*****This sets the spell back to its original state*****
Function setOriginalSpellStatsConc()
	loopCounter = 0
	while loopCounter < _SChSpell.GetNumEffects()
		_SChMagEffTrace[loopCounter] = _SChSpell.GetNthEffectMagicEffect(loopCounter)
		if _SChMagEffTrace[loopCounter] != None
			_SChSpell.SetNthEffectMagnitude(loopCounter, _SChMagni[loopCounter])
			_SChSpell.SetNthEffectDuration(loopCounter, _SChDur[loopCounter])
			_SChSpell.SetNthEffectArea(loopCounter, _SChArea[loopCounter])
			Debug.Trace("_SChpConc: Index "+loopCounter+" Magic Effect "\
			+_SChMagEffTrace[loopCounter].GetName()+" reverts magnitude to "\
			+_SChSpell.GetNthEffectMagnitude(loopCounter)+\
			" with old area of "+_SChSpell.GetNthEffectArea(loopCounter)+" and old duration of "\
			+_SChSpell.GetNthEffectDuration(loopCounter))
		endif
		loopCounter += 1
	endwhile
EndFunction

;*****This sets the accumulation of spell strength*****
Float newMag
Function setNewSpellStatsConc()
	loopCounter = 0
	while loopCounter < _SChSpell.GetNumEffects()
		_SChMagEffTrace[loopCounter] = _SChSpell.GetNthEffectMagicEffect(loopCounter)
		newMag = _SChMagni[loopCounter] + (_SChMagni[loopCounter] * (accumCharge * 0.25))
		if _SChMagEffTrace[loopCounter] != None
			_SChSpell.SetNthEffectMagnitude(loopCounter, newMag)
			if(_SChArea[loopCounter] != 0)
				_SChSpell.SetNthEffectArea(loopCounter, _SChArea[loopCounter] + (accumCharge as int))
			endif
			if(currentCharge != 0)
				_SChSpell.SetNthEffectDuration(loopCounter, _SChDur[loopCounter] * currentCharge)
			endif
			Debug.Trace("_SChpConc: Index "+loopCounter+" Magic Effect "\
			+_SChMagEffTrace[loopCounter].GetName()+" has new magnitude of "\
			+_SChSpell.GetNthEffectMagnitude(loopCounter)+" with area of "\
			+_SChSpell.GetNthEffectArea(loopCounter)+" and new duration of "\
			+_SChSpell.GetNthEffectDuration(loopCounter))
		endif
		loopCounter += 1
	endwhile
EndFunction

String Property spellNode Auto
String Property castVarBool Auto
Projectile Property _SchBaseSpellProj Auto
ObjectReference Property _SChFiredSpellProj Auto

State preCharge
	Event OnBeginState()
		;_SChIsChargingGlobal.SetValue(0)
		wiseman.DispelSpell(_SChDecreaseSpeedSpell)
		RegisterForActorAction(1)
		chargingFlag = false
		NetImmerse.SetNodeScale(wiseman, spellNode, 1, false)
		NetImmerse.SetNodeScale(wiseman, spellNode, 1, true)
		if(_SChWillDecreaseSpeedMultGlobal.GetValue()==1)
			wiseman.SetAV("SpeedMult", wiseman.GetAV("SpeedMult") + speedDecrease)
		endif
		usesGamePad = false
	EndEvent
	Event OnActorAction(int actionType, Actor akActor, Form source, int slot)
		if(akActor == wiseman && _SChIsEnabledGlobal.GetValue()==1 && _SChIsChargingGlobal.GetValue()==0)
			if(actionType==1)
			
				;*****Reset the spell's stats before committing another charging cycle*****
				;***Hmm, maybe this isn't needed?***
				if(_SChSpell != none)
					setOriginalSpellStatsConc()
				endif
				
				;*****Initialize*****
				_SChSpell = source as Spell
				_SChMagEff = _SChSpell.GetNthEffectMagicEffect(_SChSpell.GetCostliestEffectIndex())
				_SchBaseSpellProj = _SChMagEff.GetProjectile()
				_SChMagicSkill = _SChMagEff.GetAssociatedSkill() ;Get the magic skill, for later use
				if(_SChMagEff.GetCastingType()==2)	
					_SChFiredSpellProj = Game.FindClosestReferenceOfTypeFromRef(_SchBaseSpellProj, wiseman, 2000)
					getOriginalSpellStatsConc()
					getChargePlay(_SChSpell, _SChMagicSkill)
					if(!slot) ;left
						castVarBool = "IsCastingLeft"
						_SChKey = Input.GetMappedKey("Left Attack/Block")
						_SChKeyGamePad = Input.GetMappedKey("Left Attack/Block", 2)
						Debug.Trace("_SChpConconc: Left hand spell")
						spellNode = "NPC L MagicNode [LMag]"
					else ;right
						castVarBool = "IsCastingRight"
						_SChKey = Input.GetMappedKey("Right Attack/Block")
						_SChKeyGamePad = Input.GetMappedKey("Right Attack/Block", 2)
						Debug.Trace("_SChpConconc: Right hand spell")
						spellNode = "NPC R MagicNode [RMag]"
					endif
					chargingFlag = true
					UnregisterForActorAction(1)
					Debug.Trace("_SChpConconc-goto: Proceed to charging")
					if(_SChMagEff.GetCastingType()==2 && _SChCanChargeConcGlobal.GetValue()==1)
						GoToState("concCharging")
					else
						GoToState("busy")
					endif
				endif
			endif
		endif
	EndEvent
	
EndState

State busy
	Event OnBeginState()
		GoToState("preCharge")
	EndEvent
EndState

Int Property currentCharge Auto
Float Property chargeLimit Auto
bool Property StillCharging Auto
Float Property accumCharge Auto
Float Property wisemanMagSkill Auto
Float Property chargeMultiplier Auto
Float Property chargeTimer Auto
Float Property costWeight Auto
Float Property totalWait Auto

;*****These variables change the balancing of the mod*****
GlobalVariable Property _SChChargeLimitGlobal Auto
GlobalVariable Property _SChChargeMultGlobal Auto
GlobalVariable Property _SChChargeTimerGlobal Auto
GlobalVariable Property _SChIsGamePadGlobal Auto
GlobalVariable Property _SChMagickDamageGlobal Auto
Float Property currentScale Auto
Float Property incrementerTest Auto
Float Property speedDecrease Auto
Float Property magDamage Auto
Float Property timeNow Auto

;*****Spell to decrease move speed*****
Spell Property _SChDecreaseSpeedSpell Auto

;*****These variables determine difficulty options of the mod*****
GlobalVariable Property _SChWillIncrementTimerGlobal Auto
GlobalVariable Property _SChWillDecreaseSpeedMultGlobal Auto
GlobalVariable Property _SChWillBlowMeUpGlobal Auto
GlobalVariable Property _SChSpeedDecreaseIncrementGlobal Auto
GlobalVariable Property _SChConcChargeCycleMult Auto

;*****These variables determine miscellaneous options of the mod*****
GlobalVariable Property _SChSizeIncrementGlobal Auto

Event OnInit()
EndEvent

State concCharging
	Event OnBeginState()
		
		incrementerTest = 0
		timeNow = Game.GetRealHoursPassed()
		Debug.Trace("_SChpConconc: "+_SChSpell.GetName()+" Now charging a concentration spell!")
		Debug.Trace("_SChpConconc: Time now is "+timeNow)
		RegisterForActorAction(9) ;sheathe
		RegisterForActorAction(2) ;spell fire
		_SCChargeLevelGlobal.SetValue(0)
		wisemanMagSkill = wiseman.GetAV(_SChMagicSkill)
		currentCharge = 0
		accumCharge = 0
		speedDecrease = 0
		currentScale = 1
		
		magDamage = (_SChSpell.GetEffectiveMagickaCost(wiseman) * _SChMagickDamageGlobal.GetValue())
		
		;*****Default settings can be manipulated here*****
		;***Charge limit is set to 5 by default***
		chargeLimit = _SChChargeLimitGlobal.GetValue() + (wisemanMagSkill / 25)
		;***Charge multiplier is set to 2.5% of the magic skill by default; higher means stronger accumulation***
		chargeMultiplier = _SChChargeMultGlobal.GetValue()
		;***Charge time multiplier, set to 1.5% of magic skill by default; higher means faster charge**
		chargeTimer = _SChChargeTimerGlobal.GetValue()
		;***Cost weight is the amount to reduce charge time based on magic cost***
		costWeight = _SChSpell.GetEffectiveMagickaCost(wiseman) * chargeTimer
		;***Total wait determines the actual wait time of a charge cycle; had to use a variable to account for negative values***
		totalWait = (4 - (wisemanMagSkill * chargeTimer)) - costWeight
		if(totalWait <= 0)
			totalWait = 1
		endif
			
		while(chargingFlag == true && currentCharge < chargeLimit)
			;incrementerTest += 0.00005
			;Debug.Trace("_SChpConconc: Incrementer at "+incrementerTest+" and total wait is "+totalWait)
			if(_SChIsGamePadGlobal.GetValue()==0)
				if(!Input.IsKeyPressed(_SChKey)) ;This actually fixes the charging bug
				;if(!wiseman.GetAnimationVariableBool(castVarBool)) ;This actually fixes the charging bug
					cleanUp("no longer charging - gamepad off")
				endif
			else
				;if(!Input.IsKeyPressed(_SChKeyGamePad)&&(Game.UsingGamePad())) ;This actually fixes the charging bug
				if(!wiseman.GetAnimationVariableBool(castVarBool))
					cleanUp("no longer charging - gamepad on")
				endif
			endif
			;Debug.Trace("_SChpConconc: Time gap is "+(Game.GetRealHoursPassed() - timeNow)*10000+" vs total wait of "+totalWait)
			;if(chargingFlag && (incrementerTest>totalWait) && wiseman.GetAV("Magicka") >= magDamage)
			if(chargingFlag && ((Game.GetRealHoursPassed() - timeNow) * _SChConcChargeCycleMult.GetValue() >totalWait) && wiseman.GetAV("Magicka") >= magDamage)
				if(_SChFiredSpellProj == none)
					_SChFiredSpellProj = Game.FindClosestReferenceOfTypeFromRef(_SchBaseSpellProj, wiseman, 2000)
				endif
				;Utility.Wait(totalWait)
				_SCChargeLevelGlobal.SetValue(1)
				incrementerTest = 0
				timeNow = Game.GetRealHoursPassed()
				currentCharge += 1
				currentScale += _SChSizeIncrementGlobal.GetValue()
				
				accumCharge += wisemanMagSkill * chargeMultiplier
				Debug.Trace("_SChpConconc: "+_SChSpell.GetName()+" Now getting accumulated power, "+accumCharge)
				
				;*****Accumulate magnitude and increase duration*****
				setNewSpellStatsConc()
				
				;*****Increase wait time*****
				if(_SChWillIncrementTimerGlobal.GetValue()==1)
					totalWait += (totalWait / 3)
				endif
				
				;*****Decrease walking speed*****
				speedDecrease += _SChSpeedDecreaseIncrementGlobal.GetValue()
				_SChDecreaseSpeedSpell.SetNthEffectMagnitude(0, speedDecrease)
				_SChDecreaseSpeedSpell.SetNthEffectDuration(0, 300)
				if(_SChWillDecreaseSpeedMultGlobal.GetValue()==1)
					if(wiseman.HasSpell(_SChDecreaseSpeedSpell))
						wiseman.DispelSpell(_SChDecreaseSpeedSpell)
					endif
					if(wiseman.GetAV("SpeedMult") >= 0.4)
						;_SChDecreaseSpeedSpell.Cast(wiseman, wiseman)
						wiseman.SetAV("SpeedMult", wiseman.GetAV("SpeedMult") - speedDecrease)
					endif
				endif
				
				;*****Play the sound and shader; hurt magicka*****
				if(_SChChargeSizeGlobal.GetValue()==0)
					ChargingShader.Play(wiseman, 0.1)
				endif
				SoundPlay.Play(wiseman)
				wiseman.DamageAV("Magicka", magDamage)	
				
				;*****Increase size of spell on hand*****
				if(_SChChargeSizeGlobal.GetValue()==1)
					NetImmerse.SetNodeScale(wiseman, spellNode, currentScale, false)
					NetImmerse.SetNodeScale(wiseman, spellNode, currentScale, true)
					if(_SChFiredSpellProj != none)
						_SChFiredSpellProj.SetScale(1+currentScale)
					endif
				endif
				;*****Set the global's value to be used be other mods*****
				;_SCChargeLevelGlobal.SetValue(currentCharge) 
				;Utility.Wait(0.3)
			endif
		endWhile
		
		;*****These would work for concentration I think*****
		;_SChFiredSpellProj = Game.FindClosestReferenceOfTypeFromRef(_SchBaseSpellProj, wiseman, 20000)
		;_SChFiredSpellProj.SetScale(_SChFiredSpellProj.GetScale()+currentScale)
		;if(!chargingFlag)
			Debug.Trace("_SChpConconc-cast:  "+_SChSpell.GetName()+" Spell has been cast, reverting spell strength")
			cleanUp("end casting")
		;endif
 	
	EndEvent
	
	Event OnActorAction(int actionType, Actor akActor, Form source, int slot)
		if(akActor==wiseman)
			;*****Re-initialize if cancelling*****
			if(actionType==9)
				Debug.Trace("_SChpConconc-cancelled: "+_SChSpell.GetName()+" Spell has been cancelled, reverting spell strength")
				cleanUp("cancelled casting")
			endif
			if(actionType==2)
			
			endif
		endif
	EndEvent
	
EndState


Function cleanUp(string cleanPoint)
	Debug.Trace("_SchPConc-CleanUp: Cleaned up from "+cleanPoint)
	wiseman.DispelSpell(_SChDecreaseSpeedSpell)
	chargingFlag = false
	currentCharge = 0
	accumCharge = 0
	_SChIsChargingGlobal.SetValue(0)
	UnregisterForActorAction(9)
	UnregisterForActorAction(2)
	GoToState("preCharge")
EndFunction

;*****This area defines the effects such as shaders and sounds that would play*****
EffectShader Property AbsorbHealthFXS Auto
EffectShader Property FireCloakFXShader Auto
EffectShader Property AtronachFrostFXS Auto
EffectShader Property FrostSpikeFXShader Auto
EffectShader Property ShockPlayerCloakFXShader Auto
EffectShader Property IllusionPositiveFXS Auto
EffectShader Property EnchDamageBlueFXS Auto
EffectShader Property GhostFXShaderNightingale Auto
EffectShader Property ChargingShader Auto

Sound Property MAGRestorationFFFire Auto
Sound Property MAGFirebolt03Fire Auto
Sound Property MAGFrostbiteFire2D Auto
Sound Property MAGShockFFFire Auto
Sound Property MAGIllusionNightEyeOn Auto
Sound Property MAGCloakOut Auto
Sound Property MAGConjurePortal Auto
Sound Property SoundPlay Auto

Keyword Property MagicDamageFire Auto
Keyword Property MagicDamageShock Auto
Keyword Property MagicDamageFrost Auto

Function getChargePlay(Spell mySpell, String assocSkill)
	Debug.Trace("SchPC-getCharge: My spell is "+mySpell.GetName()+", while skill is "+assocSkill)
	if (mySpell.HasKeyword(MagicDamageShock) && assocSkill=="Destruction")
		ChargingShader = ShockPlayerCloakFXShader
		SoundPlay = MAGShockFFFire
	elseif(mySpell.HasKeyword(MagicDamageFrost) && assocSkill=="Destruction")
		ChargingShader = FrostSpikeFXShader
		SoundPlay = MAGFrostbiteFire2D
	elseif(mySpell.HasKeyword(MagicDamageFire) && assocSkill=="Destruction")
		ChargingShader = FireCloakFXShader
		SoundPlay = MAGFirebolt03Fire
	elseif(assocSkill == "Restoration")
		ChargingShader = AbsorbHealthFXS
		SoundPlay = MAGRestorationFFFire
	elseif(assocSkill == "Illusion")
		ChargingShader = IllusionPositiveFXS
		SoundPlay = MAGIllusionNightEyeOn
	elseif(assocSkill == "Alteration")
		ChargingShader = EnchDamageBlueFXS
		SoundPlay = MAGCloakOut
	elseif(assocSkill == "Conjuration")
		ChargingShader = GhostFXShaderNightingale
		SoundPlay = MAGConjurePortal
	endif
EndFunction

