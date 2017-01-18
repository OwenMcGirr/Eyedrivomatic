// 
// 
// 


#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "ResetAction.h"
#include "SendStatusAction.h"
#include "MoveServoAction.h"
#include "State.h"
#include "ToggleSwitchAction.h"


void ResetActionClass::execute(const char * parameters)
{
	//cancel the servo and switch timers. Doesn't reset the servo or switch. That will be done by the state object. Prevents a response.
	MoveServoAction.cancel(false); 
	ToggleSwitchAction.cancel_all(false); 
	State.reset();
}


ResetActionClass ResetAction;
