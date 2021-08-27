Scriptname _SChPractitionerScriptv2 extends activemagiceffect  

GlobalVariable Property _SCChargeLevelGlobal Auto
GlobalVariable Property _SCChargeElementID Auto
GlobalVariable Property _SChIsEnabledGlobal Auto
GlobalVariable Property _SChCanChargeConcGlobal Auto
GlobalVariable Property _SChChargeSizeGlobal Auto
GlobalVariable Property _SChIsChargingGlobal Auto
GlobalVariable Property _SChSpeedFixGlobal Auto

Actor Property wiseman Auto

Perk Property _SChSpellChargingPerk Auto
Event OnInit()
EndEvent

Event OnEffectStart(Actor akTarget, Actor akCaster)
	wiseman = Game.GetPlayer()
	GoToState("preCharge")
EndEvent

String Property _SChMagicSkill Auto
Spell Property _SChSpell Auto
MagicEffect Property _SChMagEff Auto

;*****This function reverts the perk's magnitude*****
Function revertSpellMagnitude()
	_SChSpellChargingPerk.SetNthEntryValue(0, 0, 1)
	_SChSpellChargingPerk.SetNthEntryValue(1, 0, 1)
	Debug.Trace("_SCh-revert: Magnitude reverted!")
EndFunction

;*****This sets the accumulation of spell strength*****
; Float newMag
; Int newDur
Function setNewMagnitude(Float accum, int dur)
	Debug.Trace("_SCh-newMag: Applied new magnitude!")
	_SChSpellChargingPerk.SetNthEntryValue(0, 0, dur) ;Duration
	_SChSpellChargingPerk.SetNthEntryValue(1, 0, (1+accum)) ;Magnitude
EndFunction

; Float origMag
; Int origDur
; Function getOriginalMagnitude(Spell spellApply)
	; origMag = spellApply.GetNthEffectMagnitude(0)
	; origDur = spellApply.GetNthEffectDuration(0)
; EndFunction

; Function setOriginalMagnitude(Spell spellApply)
	; spellApply.SetNthEffectMagnitude(0,origMag)
	; spellApply.SetNthEffectDuration(0,origDur)
; EndFunction

; Function setNewMagnitude(Spell spellApply, Float accum, int dur)
	; newMag = origMag * (1+accum)
	; spellApply.SetNthEffectMagnitude(0,newMag)
	; newDur = origDur * dur
	; spellApply.SetNthEffectDuration(0,newDur)
; EndFunction


String Property spellNode Auto
String Property castVarBool Auto
Projectile Property _SchBaseSpellProj Auto
ObjectReference Property _SChFiredSpellProj Auto

int spellSlot

State preCharge
	Event OnBeginState()
		RegisterForActorAction(1)
		RegisterForActorAction(2)
		RegisterForActorAction(10)
		Debug.Trace("_SCh-Start: Spell charging v2 script!")
		;Debug.Notification("_SCh-Start: Spell charging v2 script!")
		chargingFlag = false
		;*****Initialize*****
		NetImmerse.SetNodeScale(wiseman, "NPC L MagicNode [LMag]", 1, false)
		NetImmerse.SetNodeScale(wiseman, "NPC L MagicNode [LMag]", 1, true)
		NetImmerse.SetNodeScale(wiseman, "NPC R MagicNode [RMag]", 1, false)
		NetImmerse.SetNodeScale(wiseman, "NPC R MagicNode [RMag]", 1, true)
	EndEvent
	Event OnActorAction(int actionType, Actor akActor, Form source, int slot)
		if(akActor == wiseman && _SChIsEnabledGlobal.GetValue()==1)
			if(actionType==1)
			
				;*****Reset spell magnitude*****
				revertSpellMagnitude()
				
				; if(_SChSpell!=none)
					; setOriginalMagnitude(_SChSpell)
				; endif
				
				;*****Initialize*****
				NetImmerse.SetNodeScale(wiseman, "NPC L MagicNode [LMag]", 1, false)
				NetImmerse.SetNodeScale(wiseman, "NPC L MagicNode [LMag]", 1, true)
				NetImmerse.SetNodeScale(wiseman, "NPC R MagicNode [RMag]", 1, false)
				NetImmerse.SetNodeScale(wiseman, "NPC R MagicNode [RMag]", 1, true)
				_SChSpell = source as Spell
				_SChMagEff = _SChSpell.GetNthEffectMagicEffect(_SChSpell.GetCostliestEffectIndex())
				_SchBaseSpellProj = _SChMagEff.GetProjectile()
				_SChMagicSkill = _SChMagEff.GetAssociatedSkill() ;Get the magic skill, for later use
				
				getChargePlay(_SChSpell, _SChMagicSkill)
				;getOriginalMagnitude(_SChSpell)
				
				; if(wiseman.GetAV("SpeedMult") != 100 && _SChSpeedFixGlobal.GetValue()==1)
					; wiseman.SetAV("SpeedMult", 100)
					; wiseman.ForceAV("SpeedMult", 100)
				; endif
				
				if(!slot) ;left
					castVarBool = "IsCastingLeft"
					Debug.Trace("_SChPC: Left hand spell")
					spellNode = "NPC L MagicNode [LMag]"
				else ;right
					castVarBool = "IsCastingRight"
					Debug.Trace("_SChPC: Right hand spell")
					spellNode = "NPC R MagicNode [RMag]"
				endif
				spellSlot = slot
				chargingFlag = true
				UnregisterForActorAction(1)
				Debug.Trace("_SchpC-goto: Proceed to charging")
				;GoToState("nowCharging")
				;*****Charging Initialization*****
				_SChIsChargingGlobal.SetValue(1)
				timeNow = Game.GetRealHoursPassed()
				Debug.Trace("_SchpC: "+_SChSpell.GetName()+" Now charging")
				Debug.Trace("_SchPC: Time now is "+timeNow)
				RegisterForActorAction(9) ;sheathe
				RegisterForActorAction(2) ;spell fire
				_SCChargeLevelGlobal.SetValue(0)
				wisemanMagSkill = wiseman.GetAV(_SChMagicSkill)
				currentCharge = 0
				accumCharge = 0
				speedDecrease = 0
				currentScale = 1
				Utility.Wait(_SChSpell.GetCastTime()) ;wait for the spell to be ready first
				
				magDamage = (_SChSpell.GetEffectiveMagickaCost(wiseman) * _SChMagickDamageGlobal.GetValue())
				
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
				if(_SChMagEff.GetCastingType()==2)
					totalWait = totalWait * 0.40
				endif
				if(totalWait <= 0)
					totalWait = 1
				endif
				if(!wiseman.GetAnimationVariableBool(castVarBool))
					cleanUp("changed my mind")
				endif
				RegisterForSingleUpdate(totalWait)
			endif
			if(source as Spell == _SChSpell && slot == spellSlot && _SChMagEff.GetCastingType()==2 \
			&& !wiseman.GetAnimationVariableBool(castVarBool))
				cleanUp("done")
			endif
			if(actionType==2)
				if(source as Spell == _SChSpell && slot == spellSlot && _SChMagEff.GetCastingType()!=2)
					cleanUp("spell fire")
				elseif(source as Spell == _SChSpell && slot != spellSlot)
					revertSpellMagnitude()
					;setOriginalMagnitude(_SChSpell)
				endif
			endif
			if(actionType==10)
				cleanUp("cancelled casting")
			endif
		endif
	EndEvent
	
	Event OnUpdate()
		;*****Charging cycle*****
		if(wiseman.GetAnimationVariableBool(castVarBool) \
					&& wiseman.GetAV("Magicka") >= magDamage && isInMenu()==false \
					&& currentCharge <= chargeLimit)
			timeNow = Game.GetRealHoursPassed()
			currentCharge += 1
			currentScale += _SChSizeIncrementGlobal.GetValue()
			
			accumCharge += wisemanMagSkill * chargeMultiplier
			Debug.Trace("_SchpC: "+_SChSpell.GetName()+" Now getting accumulated power, "+accumCharge)
			
			;*****Increase wait time*****
			if(_SChWillIncrementTimerGlobal.GetValue()==1)
				totalWait += (totalWait / 3)
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
			endif
			if(_SChMagEff.GetCastingType()==2)
				_SChFiredSpellProj = Game.FindClosestReferenceOfTypeFromRef(_SchBaseSpellProj, wiseman, 2000)
				_SChFiredSpellProj.SetScale(1+currentScale)
			endif
			;*****Accumulate magnitude and increase duration*****
			setNewMagnitude(accumCharge, currentCharge)
			;*****Set the global's value to be used be other mods*****
			if(_SChMagEff.GetCastingType()==2)
				_SCChargeLevelGlobal.SetValue(1) 
			else
				_SCChargeLevelGlobal.SetValue(currentCharge) 
			endif			
		endif
		; if(wiseman.GetAV("SpeedMult") != 100 && _SChSpeedFixGlobal.GetValue()==1)
			; wiseman.SetAV("SpeedMult", 100)
			; wiseman.ForceAV("SpeedMult", 100)
		; endif
		if(wiseman.GetAnimationVariableBool(castVarBool) && chargingFlag)
			RegisterForSingleUpdate(totalWait)
		else
			cleanUp("charge cycle")
		endif
	EndEvent	
EndState

Int Property currentCharge Auto
Float Property chargeLimit Auto
bool Property chargingFlag Auto
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

;*****These variables determine difficulty options of the mod*****
GlobalVariable Property _SChWillIncrementTimerGlobal Auto
GlobalVariable Property _SChWillBlowMeUpGlobal Auto

;*****These variables determine miscellaneous options of the mod*****
GlobalVariable Property _SChSizeIncrementGlobal Auto


Function cleanUp(string cleanPoint)
	Debug.Trace("_SchPC-CleanUp: Cleaned up from "+cleanPoint)
	chargingFlag = false
	currentCharge = 0
	accumCharge = 0
	_SChIsChargingGlobal.SetValue(0)
	UnregisterForActorAction(9)
	UnregisterForActorAction(2)
	if(_SChMagEff.GetCastingType()==2)
		_SCChargeLevelGlobal.SetValue(0) 
	endif
	; if(wiseman.GetAV("SpeedMult") != 100 && _SChSpeedFixGlobal.GetValue()==1)
		; wiseman.SetAV("SpeedMult", 100)
		; wiseman.ForceAV("SpeedMult", 100)
	; endif
	GoToState("busy")
EndFunction

State busy
	Event OnBeginState()
		NetImmerse.SetNodeScale(wiseman, "NPC L MagicNode [LMag]", 1, false)
		NetImmerse.SetNodeScale(wiseman, "NPC L MagicNode [LMag]", 1, true)
		NetImmerse.SetNodeScale(wiseman, "NPC R MagicNode [RMag]", 1, false)
		NetImmerse.SetNodeScale(wiseman, "NPC R MagicNode [RMag]", 1, true)
		GoToState("preCharge")
	EndEvent
EndState

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

;*****IsInMenu*****
bool Function isInMenu()
	if (UI.IsMenuOpen("BarterMenu"))
		return true
	elseif (UI.IsMenuOpen("Book Menu"))
		return true
	elseif (UI.IsMenuOpen("Console"))
		return true
	elseif (UI.IsMenuOpen("Console Native UI Menu"))
		return true
	elseif (UI.IsMenuOpen("ContainerMenu"))
		return true
	elseif (UI.IsMenuOpen("Crafting Menu"))
		return true
	elseif (UI.IsMenuOpen("Credits Menu"))
		return true
	elseif (UI.IsMenuOpen("Cursor Menu"))
		return true
	elseif (UI.IsMenuOpen("Dialogue Menu"))
		return true
	elseif (UI.IsMenuOpen("Fader Menu"))
		return true
	elseif (UI.IsMenuOpen("FavoritesMenu"))
		return true
	elseif (UI.IsMenuOpen("GiftMenu"))
		return true
	elseif (UI.IsMenuOpen("InventoryMenu"))
		return true
	elseif (UI.IsMenuOpen("Journal Menu"))
		return true
	elseif (UI.IsMenuOpen("Kinect Menu"))
		return true
	elseif (UI.IsMenuOpen("LevelUp Menu"))
		return true
	elseif (UI.IsMenuOpen("Loading Menu"))
		return true
	elseif (UI.IsMenuOpen("Lockpicking Menu"))
		return true
	elseif (UI.IsMenuOpen("MagicMenu"))
		return true
	elseif (UI.IsMenuOpen("Main Menu"))
		return true
	elseif (UI.IsMenuOpen("MapMenu"))
		return true
	elseif (UI.IsMenuOpen("MessageBoxMenu"))
		return true
	elseif (UI.IsMenuOpen("Mist Menu"))
		return true
	elseif (UI.IsMenuOpen("Overlay Interaction Menu"))
		return true
	elseif (UI.IsMenuOpen("Overlay Menu"))
		return true
	elseif (UI.IsMenuOpen("Quantity Menu"))
		return true
	elseif (UI.IsMenuOpen("RaceSex Menu"))
		return true
	elseif (UI.IsMenuOpen("Sleep/Wait Menu"))
		return true
	elseif (UI.IsMenuOpen("StatsMenu"))
		return true
	elseif (UI.IsMenuOpen("TitleSequence Menu"))
		return true
	elseif (UI.IsMenuOpen("Top Menu"))
		return true
	elseif (UI.IsMenuOpen("Training Menu"))
		return true
	elseif (UI.IsMenuOpen("Tutorial Menu"))
		return true
	elseif (UI.IsMenuOpen("TweenMenu"))
		return true
	else
		return false
	endif
EndFunction
