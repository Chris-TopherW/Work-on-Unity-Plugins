#pragma once

#include "VSTEffect.h"
#include "VSTi.h"

class VstHost : public VstBasicParams
{
public:
	VstHost();
	void setBlockSize(int p_blocksize);
	int loadEffect(std::string& path); //returns index
	int loadInstrument(std::string& path); //returns index
	VSTEffect& getEffect(int index);
	VSTi& getInstrument(int index);
	//void unloadEffect(int index);
	//void unloadInstrument(int index); 
	static std::vector<std::vector<float>> inputsHolder;
	static std::vector<std::vector<float>> outputsHolder;
		
private:
	std::vector<VSTEffect> audioEffects;
	std::vector<VSTi> instruments;
};

// Plugin's entry point
typedef AEffect *(*vstPluginFuncPtr)(audioMasterCallback host);
// Plugin's dispatcher function
typedef VstIntPtr(*dispatcherFuncPtr)(AEffect *effect, VstInt32 opCode,
	VstInt32 index, VstInt32 value, void *ptr, float opt);
// Plugin's getParameter() method
typedef float(*getParameterFuncPtr)(AEffect *effect, VstInt32 index);
// Plugin's setParameter() method
typedef void(*setParameterFuncPtr)(AEffect *effect, VstInt32 index, float value);
// Plugin's processEvents() method
typedef VstInt32(*processEventsFuncPtr)(VstEvents *events);
// Plugin's process() method
typedef void(*processFuncPtr)(AEffect *effect, float **inputs,
	float **outputs, VstInt32 sampleFrames);
