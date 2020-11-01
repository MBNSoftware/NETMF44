/*
 * Buzz Click board driver
 * 
 * Version 2.0 :
 *  - Conformance to new namespaces and organization
 *  
 * Version 1.0 :
 *   - Initial revision coded by Niels Jakob Buch
 *   - Thanks to GHI for the great pieces on Tone, Melody, etc.
 *    
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Hardware.PWM
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright 2014 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License..
 */

using System.Reflection;
using MBN.Enums;
using MBN.Exceptions;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;
using System.Collections;

namespace MBN.Modules
{
    /// <summary>
	/// Main class for Buzz module for Microbus.Net driver 
	/// </summary>
	/// <example> This sample shows a basic usage of the Buzzer Click board.
    /// <code language="C#">
	/// public class Program
    /// {
    ///     static BuzzerClick _buzzer;
    ///     
    ///     // This method is run when the mainboard is powered up or reset.   
    ///     public static void Main()
    ///     {
    ///         _buzzer = new BuzzerClick(Hardware.SocketOne);
    ///         var note = new BuzzerClick.MusicNote(BuzzerClick.Tone.C4, 400);
    /// 
    ///         _buzzer.AddNote(note);
    /// 
    ///         // up
    ///         PlayNote(BuzzerClick.Tone.C4);
    ///         PlayNote(BuzzerClick.Tone.D4);
    ///         PlayNote(BuzzerClick.Tone.E4);
    ///         PlayNote(BuzzerClick.Tone.F4);
    ///         PlayNote(BuzzerClick.Tone.G4);
    ///         PlayNote(BuzzerClick.Tone.A4);
    ///         PlayNote(BuzzerClick.Tone.B4);
    ///         PlayNote(BuzzerClick.Tone.C5);
    /// </code>
    /// </example>
    public class BuzzerClick : IDriver
    {
        // PWM channel for alternating the pulse at given frequencies.
        private readonly PWM _buzzPwm;
		private Melody _playList;
		private bool _running;
		private Thread _playbackThread;

        /// <summary>
        /// Main class constructor for Buzzer 
        /// <para><b>Pins used :</b> Cs, Pwm</para>
        /// </summary>
        /// <param name="socket">The socket on which the Buzzer Click board is plugged on MikroBus.Net</param>
        /// <exception cref="System.InvalidOperationException">Thrown if some pins are already in use by another board on the same socket</exception>
        public BuzzerClick(Hardware.Socket socket)
        {
            try
            {
                Hardware.CheckPins(socket, socket.Cs, socket.Pwm);

                _buzzPwm = new PWM(socket.PwmChannel, 10000, 0, false);
                _buzzPwm.Start();
                _playList = new Melody();
            }
            // Catch only the PinInUse exception, so that program will halt on other exceptions
            // Send it directly to caller
            catch (PinInUseException) { throw new PinInUseException(); }
        }

            
#region Melody-stuff
		/// <summary>
		/// Represents a list of notes to play in sequence.
		/// </summary>
        /// <example> This sample shows how to use the Melody class.
        /// <code language="C#">
        /// var melody = new BuzzerClick.Melody();
        ///
        ///    // up
        ///    melody.Add(BuzzerClick.Tone.C4, 200);
        ///    melody.Add(BuzzerClick.Tone.D4, 200);
        ///    melody.Add(BuzzerClick.Tone.E4, 200);
        ///    melody.Add(BuzzerClick.Tone.F4, 200);
        ///    melody.Add(BuzzerClick.Tone.E4, 200);
        ///    melody.Add(BuzzerClick.Tone.C4, 200);
        ///
        ///    _buzzer.Play(melody);
        /// </code>
        /// </example>
		public class Melody
		{
			private readonly Queue _list;

			/// <summary>
			/// Creates a new instance of a melody.
			/// </summary>
            /// <example> This sample shows how to use the Melody.Melody() method.
            /// <code language="C#">
            /// var melody = new BuzzerClick.Melody();
            /// melody.Add(BuzzerClick.Tone.C4, 200);
            /// </code>
            /// </example>
			public Melody()
			{
				_list = new Queue();
			}

			/// <summary>
			/// Adds a new note to the list to play.
			/// </summary>
			/// <param name="frequency">The frequency of the note.</param>
			/// <param name="milliseconds">The duration of the note.</param>
            /// <example> This sample shows how to use the Melody.Add() method.
            /// <code language="C#">
            /// var melody = new BuzzerClick.Melody();
            /// melody.Add(12000, 200);
            /// </code>
            /// </example>
			public void Add(int frequency, int milliseconds)
			{
				Add(new Tone(frequency), milliseconds);
			}

			/// <summary>
			/// Adds a new note to the list to play.
			/// </summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration of the note.</param>
            /// <example> This sample shows how to use the Melody.Add() method.
            /// <code language="C#">
            /// var melody = new BuzzerClick.Melody();
            /// melody.Add(BuzzerClick.Tone.C4, 200);
            /// </code>
            /// </example>
			public void Add(Tone tone, int duration)
			{
				Add(new MusicNote(tone, duration));
			}

			/// <summary>
			/// Adds an existing note to the list to play.
			/// </summary>
			/// <param name="note">The note to add.</param>
			public void Add(MusicNote note)
			{
				_list.Enqueue(note);
			}

			/// <summary>
			/// Gets the next note to play from the melody.
			/// </summary>
			/// <returns></returns>
			public MusicNote GetNextNote()
			{
				if (_list.Count == 0)
					throw new Exception("No notes added.");

				return (MusicNote)_list.Dequeue();
			}

			/// <summary>
			/// Gets the number of notes left to play in the melody.
			/// </summary>
			public int NotesRemaining
			{
				get
				{
					return _list.Count;
				}
			}

			/// <summary>
			/// Removes all notes from the melody.
			/// </summary>
			public void Clear()
			{
				_list.Clear();
			}
		}

		/// <summary>
		/// Class that holds and manages notes that can be played.
		/// </summary>
		public class Tone
		{
			/// <summary>
			/// Frequency of the note in hertz
			/// </summary>
			public double Freq;

			/// <summary>
			/// Constructs a new Tone.
			/// </summary>
			/// <param name="freq">The frequency of the tone.</param>
			public Tone(double freq)
			{
				Freq = freq;
			}

			/// <summary>
			/// A "rest" note, or a silent note.
			/// </summary>
			public static readonly Tone Rest = new Tone(0.0);

			#region 4th Octave
			/// <summary>
			/// C in the 4th octave. Middle C.
			/// </summary>
			public static readonly Tone C4 = new Tone(261.626);

			/// <summary>
			/// D in the 4th octave.
			/// </summary>
			public static readonly Tone D4 = new Tone(293.665);

			/// <summary>
			/// E in the 4th octave.
			/// </summary>
			public static readonly Tone E4 = new Tone(329.628);

			/// <summary>
			/// F in the 4th octave.
			/// </summary>
			public static readonly Tone F4 = new Tone(349.228);

			/// <summary>
			/// G in the 4th octave.
			/// </summary>
			public static readonly Tone G4 = new Tone(391.995);

			/// <summary>
			/// A in the 4th octave.
			/// </summary>
			public static readonly Tone A4 = new Tone(440);

			/// <summary>
			/// B in the 4th octave.
			/// </summary>
			public static readonly Tone B4 = new Tone(493.883);

			#endregion 4th Octave

			#region 5th Octave

			/// <summary>
			/// C in the 5th octave.
			/// </summary>
			public static readonly Tone C5 = new Tone(523.251);

			#endregion 5th Octave
		}

		/// <summary>
		/// Class that describes a musical note, containing a tone and a duration.
		/// </summary>
		public class MusicNote
		{
			/// <summary>
			/// The tone of the note.
			/// </summary>
			public Tone Tone;
			/// <summary>
			/// The duration of the note.
			/// </summary>
			public int Duration;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="tone">The tone of the note.</param>
			/// <param name="duration">The duration that the note should be played.</param>
			public MusicNote(Tone tone, int duration)
			{
				Tone = tone;
				Duration = duration;
			}
		}
#endregion

        /// <summary>
		/// Plays the given frequency indefinitely.
		/// </summary>
		/// <param name="frequency">The frequency to play.</param>
		public void Play(int frequency)
		{
			_playList.Clear();
			_playList.Add(frequency, int.MaxValue);
			Play();
		}

		/// <summary>
		/// Plays the given tone indefinitely.
		/// </summary>
		/// <param name="tone">The tone to play.</param>
		public void Play(Tone tone)
		{
			Play((int)tone.Freq);
		}

		/// <summary>
		/// Plays the melody.
		/// </summary>
		/// <param name="melody">The melody to play.</param>
		public void Play(Melody melody)
		{
			_playList = melody;
			Play();
		}

		/// <summary>
		/// Starts note playback of the notes added using AddNote(). Returns if it made any change.
		/// </summary>
		/// <returns>Returns true if notes were not playing and they were started. False if notes were already being played.</returns>
		public bool Play()
		{
            if (_running)
                Stop();

			// Make sure the queue is not empty and we are not currently playing it.
			if (_playList.NotesRemaining > 0)
			{
				_running = true;

				_playbackThread = new Thread(PlaybackThread);
				_playbackThread.Start();
			}

			return true;
		}

		private void PlaybackThread()
		{
			while (_running && _playList.NotesRemaining > 0)
			{
				// Get the next note.
				MusicNote currNote = _playList.GetNextNote();

				// Set the tone and sleep for the duration
				SetTone(currNote.Tone);

				Thread.Sleep(currNote.Duration);
			}

			SetTone(Tone.Rest);

			_running = false;
		}

		private void SetTone(Tone tone)
		{
            _buzzPwm.Stop();
            if (Math.Abs(tone.Freq) < Double.Epsilon)
			{
				return;
			}
            _buzzPwm.Frequency = ((int)tone.Freq);
            _buzzPwm.DutyCycle = 0.5;
			_buzzPwm.Start();
		}

		/// <summary>
		/// Stops note playback. Returns if it made any change.
		/// </summary>
		public void Stop()
		{
            if (_playbackThread != null)
            {
                _playbackThread.Abort();
                _playbackThread = null;
            }

            _running = false;
            _buzzPwm.Stop();
		}

		/// <summary>
		/// Adds a note to the queue to be played
		/// </summary>
		/// <param name="note">The note to be added, which describes the tone and duration to be played.</param>
		public void AddNote(MusicNote note)
		{
			_playList.Add(note);
		}

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module has no power modes feature.</exception>
        public PowerModes PowerMode
        {
            get { return PowerModes.On; }
            set
            {
                throw new NotImplementedException("PowerMode");
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        /// <example> This sample shows how to use the DriverVersion property.
        /// <code language="C#">
        ///             Debug.Print ("Current driver version : "+_buzzer.DriverVersion);
        /// </code>
        /// </example>
        /// <value>
        /// The driver version.
        /// </value>
        public Version DriverVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        /// <summary>
        /// Herited from the IDriver interface but not used by this module.
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if Reset has been acknowledged, false otherwise.</returns>
        /// <exception cref="System.NotImplementedException">Thrown because this module has no Reset feature.</exception>
        public bool Reset(ResetModes resetMode)
        {
            throw new NotImplementedException("Reset");
        }
    }
}


