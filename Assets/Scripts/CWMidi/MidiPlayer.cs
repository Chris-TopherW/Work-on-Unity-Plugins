﻿using System.Collections.Generic;
using UnityEngine;

namespace cwMidi
{
    public static class MidiPlayer
    {
        private static double metronomeStartTime = 0.0;
        public static int deviceNum = 0;
        private static double previousEventMS = 0.0;
        public static Queue<MidiMessage> mesOutBuff; //swap this for normal list 
        private static double updateLookAhead = 1000; //ms
        private static double startTime = 0.0;
        private static bool hasStarted = false;

        public static int Start()
        {
            if (!hasStarted)
            {
                hasStarted = true;
                mesOutBuff = new Queue<MidiMessage>();
                startTime = AudioSettings.dspTime;
                return PortMidi.main_test_output();
            }
            else
                return -1;
        }

        public static void PlayScheduled(MidiMessage p_message, double p_time)
        {
            if (p_time < AudioSettings.dspTime)
            {
                Debug.Log("<color=yellow>Warning:</color> time must not be in past!");
                return;
            }
            double theTime = AudioSettings.dspTime;

            PortMidi.midiEvent(p_message.getStatusByte(), p_message.getByteOne(), p_message.getByteTwo(), 
                             (int)((p_time - theTime) * 1000.0));
            previousEventMS = (p_time - theTime) * 1000.0;
        }

        public static void Play(MidiMessage p_message)
        {
            if(Midi.debugLevel > 4) Debug.Log("Add note to play " + p_message.getByteOne()); 

            PortMidi.midiEvent(p_message.getStatusByte(), p_message.getByteOne(), p_message.getByteTwo(), 0);
        }

        public static void PlayTrackNext(MidiTrack p_track)
        {
            for(int _notes = 0; _notes < p_track.getNumNotes(); _notes++)
            {
                PlayNext(p_track.getNote(_notes));
            }
        }

        public static void PlayNext(MidiMessage p_message)
        {
            /*if (Midi.debugLevel > 4) */Debug.Log("Add note to play " + p_message.getByteOne() + " " + p_message.getByteTwo() + " at time: " + p_message.getAbsTimeStamp());

            mesOutBuff.Enqueue(p_message);
            if(p_message.getOwnerTrack() != null)
                p_message.getOwnerTrack().trackPPQAbsolutePos = p_message.getAbsTimeStamp(); //this sets write head for ppq
        }

        public static void PlayTrack(MidiTrack p_track)
        {
            if (Midi.debugLevel > 3) Debug.Log("Play Midi track");

            resetMidiEventClock();
            p_track.trackPPQAbsolutePos = 0;
            for (int _notes = 0; _notes < p_track.getNumNotes(); _notes++)
            {
                //hmm, not quite...
                PlayNext(p_track.getNote(_notes));
            }
        }

        public static void Update()
        {
            double currentTime = (AudioSettings.dspTime - startTime) * 1000.0;
            if(mesOutBuff.Count > 0)
            {
                MidiMessage temporaryMessage = mesOutBuff.Peek();
                double msUntilEvent =  Metronome.ppqToMs(temporaryMessage.getAbsTimeStamp()) - (currentTime - metronomeStartTime);
                
                while (msUntilEvent < updateLookAhead && mesOutBuff.Count > 0)
                {
                    Debug.Log("Absolute timestamp = " + temporaryMessage.getAbsTimeStamp());
                    long msOffset = (long)(msUntilEvent);
                    UnityEngine.Debug.Log("Event time: " + msOffset);
                    if (msOffset < 0)
                    {
                        msOffset = 0;
                        Debug.Log("<color=red>Error: negative event time offset</color>");
                    }
                        
                    MidiMessage p_message = mesOutBuff.Dequeue();
                    PortMidi.midiEvent(p_message.getStatusByte(), p_message.getByteOne(), p_message.getByteTwo(),
                                     (int)(msOffset));

                    if(mesOutBuff.Count > 0)
                    {
                        temporaryMessage = mesOutBuff.Peek(); //check next element for timestamp pos
                        msUntilEvent = Metronome.ppqToMs(temporaryMessage.getAbsTimeStamp()) - (currentTime - metronomeStartTime);
                    }
                }
            }
        }

        public static void resetMidiEventClock()
        {
            metronomeStartTime = (AudioSettings.dspTime - startTime) * 1000; //m/s since start of program- provides offset
        }

        //public static void SetBPM(int p_BPM)
        //{

        //}

        //public static void SetPPQ(int p_PPQ)
        //{

        //}

        public static void StartMetronome(int p_BPM)
        {
            metronomeStartTime = AudioSettings.dspTime;
        }

        public static void Stop()
        {

        }

        public static void Stop(int p_delay)
        {

        }

        public static void Clear()
        {

        }

        public static int Shutdown()
        {
            PortMidi.shutdown();
            PortMidi.Pm_Terminate();
            startTime = 0;
            metronomeStartTime = 0;
            return 1;
        }
    }
}
