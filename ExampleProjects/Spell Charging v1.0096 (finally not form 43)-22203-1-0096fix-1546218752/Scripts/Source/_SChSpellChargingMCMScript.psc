Scriptname _SChSpellChargingMCMScript extends SKI_ConfigBase

;The ints are basically positions in the menu - these are needed to know which option was changed
;and ensuring the option remains as such
int toggleSpellChargingActive
int toggleUseGamePad
int toggleBalanceSlowSpeed
int toggleIncreaseTimer
int toggleBalanceChanceExplode
int toggleChargeConc
int toggleChangeSize
int toggleSpeedFix
int sliderChargeDamage
int sliderChargeDuration
int sliderChargeLimit
int sliderMagDam
int sliderSizeIncrement
int sliderSpeedDecrease
int sliderConcChargeCycle

bool isEnabled = true
bool isConcEnabled = true
bool willChangeSize = true
bool isGamePad = false
bool willIncrementTimer = true
bool willDecreaseSpeed = false
bool speedFix = false

GlobalVariable Property _SChIsEnabledGlobal Auto
GlobalVariable Property _SChIsGamePadGlobal Auto
GlobalVariable Property _SChChargeLimitGlobal Auto
GlobalVariable Property _SChChargeMultGlobal Auto
GlobalVariable Property _SChChargeTimerGlobal Auto
GlobalVariable Property _SChMagickDamageGlobal Auto
GlobalVariable Property _SChWillIncrementTimerGlobal Auto
GlobalVariable Property _SChWillDecreaseSpeedMultGlobal Auto
GlobalVariable Property _SChWillBlowMeUpGlobal Auto
GlobalVariable Property _SChSizeIncrementGlobal Auto
GlobalVariable Property _SChSpeedDecreaseIncrementGlobal Auto
GlobalVariable Property _SChCanChargeConcGlobal Auto
GlobalVariable Property _SChChargeSizeGlobal Auto
GlobalVariable Property _SChConcChargeCycleMult Auto
GlobalVariable Property _SChSpeedFixGlobal Auto

int function GetVersion()
    return 5 ; Current version
endFunction

event OnVersionUpdate(int a_version)
	if(a_version==2)
		Debug.Notification("Spell Charging MCM version updated to v1.02")
		willDecreaseSpeed = false
		_SChWillDecreaseSpeedMultGlobal.SetValue(0.0)
	endif
	if(a_version==3)
		Debug.Notification("Spell Charging v1.008 - MCM v1.03 - Now with charging concentration spells!")
		isConcEnabled = true
		willChangeSize = true
	endif
	if(a_version==4)
		Debug.MessageBox("Spell Charging v1.0084 - MCM v1.04 - Concentration charge options available under Charge Balancing. Please read the Spell Charging book in your inventory to force update the mod.")
	endif
	if(a_version==5)
		Debug.MessageBox("SpellCharging v1.009 - MCM v1.05 - obsolete options removed")
	endif
EndEvent

event OnPageReset(string page)
    {Called when a new page is selected, including the initial empty page}
	SetCursorFillMode(TOP_TO_BOTTOM)
	AddHeaderOption("Basics")
	toggleSpellChargingActive = AddToggleOption("Spell Charging Enabled", isEnabled)
	;toggleUseGamePad = AddToggleOption("Use Gamepad", isGamePad)
	;toggleChargeConc = AddToggleOption("Charge Concentration Spells Enabled", isConcEnabled)
	AddEmptyOption()
	
	AddHeaderOption("Charge Balancing")
	sliderChargeLimit = AddSliderOption("Base Charge Limit", _SChChargeLimitGlobal.GetValue(), "{0}")
	sliderChargeDamage = AddSliderOption("Charge Damage Multiplier", _SChChargeMultGlobal.GetValue() * 100, "{1}%")
	sliderMagDam = AddSliderOption("Magicka Damage Multiplier", _SChMagickDamageGlobal.GetValue() * 100, "{1}%")
	sliderChargeDuration = AddSliderOption("Charge Duration Multiplier", _SChChargeTimerGlobal.GetValue() * 100, "{1}%")
	;sliderConcChargeCycle = AddSliderOption("Concentration Cycle Multiplier", _SChConcChargeCycleMult.GetValue() / 1000, "{0}")
	AddEmptyOption()
	
	AddHeaderOption("Charge Difficulty")
	toggleIncreaseTimer = AddToggleOption("Increase Charge Duration per Charge", willIncrementTimer)
	;toggleBalanceSlowSpeed = AddToggleOption("*DEAD* Decrease Speed", willDecreaseSpeed)
	;sliderSpeedDecrease = AddSliderOption("Speed Decrease Multiplier", _SChSpeedDecreaseIncrementGlobal.GetValue(), "{2}")
	AddEmptyOption()
	
	AddHeaderOption("Miscellaneous")
	toggleChangeSize = AddToggleOption("Spell Size Increase Enabled", willChangeSize)
	sliderSizeIncrement = AddSliderOption("Spell Size Multiplier", _SChSizeIncrementGlobal.GetValue(), "{1}")
	;toggleSpeedFix = AddToggleOption("Speed Fix", speedFix)
	
	Debug.Trace("Enabled? "+_SChIsEnabledGlobal.GetValue())
	Debug.Trace("Gamepad? "+_SChIsGamePadGlobal.GetValue())
	Debug.Trace("Charge limit: "+_SChChargeLimitGlobal.GetValue())
	Debug.Trace("Charge damage multiplier: "+_SChChargeMultGlobal.GetValue())
	Debug.Trace("Charge timer multiplier: "+_SChChargeTimerGlobal.GetValue())
	Debug.Trace("Magick damage: "+_SChMagickDamageGlobal.GetValue())
	Debug.Trace("Increase timer? "+_SChWillIncrementTimerGlobal.GetValue())
	Debug.Trace("Decrease speed on? "+_SChWillDecreaseSpeedMultGlobal.GetValue())
	Debug.Trace("Speed decrease multiplier: "+_SChSpeedDecreaseIncrementGlobal.GetValue())
	Debug.Trace("Size increment: "+_SChSizeIncrementGlobal.GetValue())
	Debug.Trace("Will size change? "+_SChChargeSizeGlobal.GetValue())
	Debug.Trace("Charge concentration? "+_SChCanChargeConcGlobal.GetValue())
	Debug.Trace("Charge cycle multiplier?" +_SChConcChargeCycleMult.GetValue())
	
endEvent 

event OnOptionHighlight(int a_option)
	if(a_option == toggleSpellChargingActive)
		SetInfoText("This enables Spell Charging's behavior")
	elseif(a_option == toggleUseGamePad)
		SetInfoText("For players using gamepads")
	elseif(a_option == sliderChargeLimit)
		SetInfoText("This is the base value for charge limits which is affected by 1/4 of your magic skill (whole number only)")
	elseif(a_option == sliderChargeDamage)
		SetInfoText("This affects how much of your magic skill is used to accumulate charge damage - higher means more power")
	elseif(a_option == sliderMagDam)
		SetInfoText("This affects how much of the spell's magicka cost will damage your magicka per charge cycle (but there will always be a base of 1 point)")
	elseif(a_option == sliderChargeDuration)
		SetInfoText("This affects how much of your magic skill is used to reduce the duration of each charge cycle - higher means shorter duration")
	elseif(a_option == toggleIncreaseTimer)
		SetInfoText("This determines whether each charge cycle will increase the duration of each charge cycle")
	elseif(a_option == toggleBalanceSlowSpeed)
		SetInfoText("This determines whether each charge cycle will make you move slower")
	elseif(a_option == sliderSizeIncrement)
		SetInfoText("This determines the size increase of the spell being charged for each charge cycle - 0 = no size increase")
	elseif(a_option == sliderSpeedDecrease)
		SetInfoText("*DO NOT USE THIS  OPTION* This determines how much of your speed is decreased per charge cycle")
	elseif(a_option == toggleChargeConc)
		SetInfoText("This enables charging of concentration spells like Flames or Sparks")
	elseif(a_option == toggleChangeSize)
		SetInfoText("This enables changing of spell size when charging; plays a shader instead if disabled")
	elseif(a_option == sliderConcChargeCycle)
		SetInfoText("This determines how much faster or slower concentration charging is; higher means faster")
	elseif(a_option == toggleSpeedFix)
		SetInfoText("***EXPERIMENTAL!*** Fixes speed decrease/increase during charging by forcing speedmult to 100.")
	endif
	
endEvent

event OnOptionSelect(int a_option)
	if(a_option == toggleSpellChargingActive)
		isEnabled = !isEnabled
		if(isEnabled)
			_SChIsEnabledGlobal.SetValue(1)
		else
			_SChIsEnabledGlobal.SetValue(0)
		endif
		SetToggleOptionValue(a_option, isEnabled)
	elseif(a_option == toggleUseGamePad)
		isGamePad = !isGamePad
		if(isGamePad)
			_SChIsGamePadGlobal.SetValue(1)
		else
			_SChIsGamePadGlobal.SetValue(0)
		endif
		SetToggleOptionValue(a_option, isGamePad)
	elseif(a_option == toggleIncreaseTimer)
		willIncrementTimer = !willIncrementTimer
		if(willIncrementTimer)
			_SChWillIncrementTimerGlobal.SetValue(1)
		else
			_SChWillIncrementTimerGlobal.SetValue(0)
		endif
		SetToggleOptionValue(a_option, willIncrementTimer)
	elseif(a_option == toggleBalanceSlowSpeed)
		willDecreaseSpeed = !willDecreaseSpeed
		if(willDecreaseSpeed)
			_SChWillDecreaseSpeedMultGlobal.SetValue(1)
		else
			_SChWillDecreaseSpeedMultGlobal.SetValue(0)
		endif
		SetToggleOptionValue(a_option, willDecreaseSpeed)
	elseif(a_option == toggleChargeConc)
		isConcEnabled = !isConcEnabled
		if(isConcEnabled)
			_SChCanChargeConcGlobal.SetValue(1)
		else
			_SChCanChargeConcGlobal.SetValue(0)
		endif
		SetToggleOptionValue(a_option, isConcEnabled)
	elseif(a_option == toggleChangeSize)
		willChangeSize = !willChangeSize
		if(willChangeSize)
			_SChChargeSizeGlobal.SetValue(1)
		else
			_SChChargeSizeGlobal.SetValue(0)
		endif
		SetToggleOptionValue(a_option, willChangeSize)
	elseif(a_option == toggleSpeedFix)
	speedFix = !speedFix
	if(speedFix)
		_SChSpeedFixGlobal.SetValue(1)
	else
		_SChSpeedFixGlobal.SetValue(0)
	endif
	SetToggleOptionValue(a_option, speedFix)
	endif
EndEvent

event OnOptionSliderOpen(int a_option)
	{Called when the user selects a slider option}

	if (a_option == sliderChargeLimit)
		SetSliderDialogStartValue(_SChChargeLimitGlobal.GetValue())
		SetSliderDialogDefaultValue(5)
		SetSliderDialogRange(0, 15)
		SetSliderDialogInterval(1)
	elseif (a_option == sliderChargeDamage)
		SetSliderDialogStartValue(_SChChargeMultGlobal.GetValue() * 100)
		SetSliderDialogDefaultValue(2.5)
		SetSliderDialogRange(0, 150)
		SetSliderDialogInterval(0.5)
	elseif (a_option == sliderMagDam)
		SetSliderDialogStartValue(_SChMagickDamageGlobal.GetValue() * 100)
		SetSliderDialogDefaultValue(10.0)
		SetSliderDialogRange(0, 150)
		SetSliderDialogInterval(0.5)
	elseif (a_option == sliderChargeDuration)
		SetSliderDialogStartValue(_SChChargeTimerGlobal.GetValue() * 100)
		SetSliderDialogDefaultValue(1.5)
		SetSliderDialogRange(0, 150)
		SetSliderDialogInterval(0.5)
	elseif (a_option == sliderSizeIncrement)
		SetSliderDialogStartValue(_SChSizeIncrementGlobal.GetValue())
		SetSliderDialogDefaultValue(0.5)
		SetSliderDialogRange(0, 5)
		SetSliderDialogInterval(0.1)
	elseif (a_option == sliderSpeedDecrease)
		SetSliderDialogStartValue(_SChSpeedDecreaseIncrementGlobal.GetValue())
		SetSliderDialogDefaultValue(0.2)
		SetSliderDialogRange(0, 0.6)
		SetSliderDialogInterval(0.01)
	elseif (a_option == sliderConcChargeCycle)
		SetSliderDialogStartValue(_SChConcChargeCycleMult.GetValue() / 1000)
		SetSliderDialogDefaultValue(7)
		SetSliderDialogRange(1, 20)
		SetSliderDialogInterval(1)
	endIf
	ForcePageReset()
endEvent

event OnOptionSliderAccept(int a_option, float a_value)
	{Called when the user accepts a new slider value}
		
	if (a_option == sliderChargeLimit)
		_SChChargeLimitGlobal.SetValue(a_value)
		SetSliderOptionValue(a_option, a_value, "{0}")
	elseif (a_option == sliderChargeDamage)
		_SChChargeMultGlobal.SetValue(a_value / 100)
		SetSliderOptionValue(a_option, a_value, "{1}%")
	elseif (a_option == sliderMagDam)
		_SChMagickDamageGlobal.SetValue(a_value / 100)
		SetSliderOptionValue(a_option, a_value, "{1}%")
	elseif (a_option == sliderChargeDuration)
		_SChChargeTimerGlobal.SetValue(a_value / 100)
		SetSliderOptionValue(a_option, a_value, "{1}%")
	elseif (a_option == sliderSpeedDecrease)
		_SChSpeedDecreaseIncrementGlobal.SetValue(a_value)
		SetSliderOptionValue(a_option, a_value, "{2}")
	elseif (a_option == sliderSizeIncrement)
		_SChSizeIncrementGlobal.SetValue(a_value)
		SetSliderOptionValue(a_option, a_value, "{1}")
	elseif (a_option == sliderConcChargeCycle)
		_SChConcChargeCycleMult.SetValue(a_value * 1000)
		SetSliderOptionValue(a_option, a_value, "{0}")
	endIf
	ForcePageReset()
endEvent
