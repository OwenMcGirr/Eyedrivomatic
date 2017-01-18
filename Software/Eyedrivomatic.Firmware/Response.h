// Response.h

#ifndef _RESPONSE_h
#define _RESPONSE_h

#include "Message.h"

#define RESPONSE_QUEUE_SIZE 4

class ResponseClass
{
public:
	struct QueuedResponse
	{
		char buffer[WRITE_BUFFER_SIZE];
	};

	void SendResponse(const char *message);
	void SendResponse_f(const char *message, ...);
	void SendResponse_v(const char *message, va_list vl);
	void SendResponse_P(const char *message, ...);
	void QueueResponse(const char* message, ...);
	void QueueResponse_v(const char* message, va_list vl);
	void QueueResponse_P(const char* message, ...);
	void SendQueuedResponses();

private:
	volatile int8_t _queuePtrStart = 0;
	volatile int8_t _queuePtrEnd = 0;
	QueuedResponse _queuedResponse[RESPONSE_QUEUE_SIZE];
};

extern ResponseClass Response;

#endif

