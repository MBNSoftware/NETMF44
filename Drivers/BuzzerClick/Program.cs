using System.Threading;
using MBN;
using MBN.Modules;

namespace Examples
{
    public class Program
    {
        static BuzzerClick _buzz;
        
        // This method is run when the mainboard is powered up or reset.   
        public static void Main()
        {
            _buzz = new BuzzerClick(Hardware.SocketOne);
            var note = new BuzzerClick.MusicNote(BuzzerClick.Tone.C4, 400);

            _buzz.AddNote(note);

            // up
            PlayNote(BuzzerClick.Tone.C4);
            PlayNote(BuzzerClick.Tone.D4);
            PlayNote(BuzzerClick.Tone.E4);
            PlayNote(BuzzerClick.Tone.F4);
            PlayNote(BuzzerClick.Tone.G4);
            PlayNote(BuzzerClick.Tone.A4);
            PlayNote(BuzzerClick.Tone.B4);
            PlayNote(BuzzerClick.Tone.C5);

            // back down
            PlayNote(BuzzerClick.Tone.B4);
            PlayNote(BuzzerClick.Tone.A4);
            PlayNote(BuzzerClick.Tone.G4);
            PlayNote(BuzzerClick.Tone.F4);
            PlayNote(BuzzerClick.Tone.E4);
            PlayNote(BuzzerClick.Tone.D4);
            PlayNote(BuzzerClick.Tone.C4);

            // arpeggio
            PlayNote(BuzzerClick.Tone.E4);
            PlayNote(BuzzerClick.Tone.G4);
            PlayNote(BuzzerClick.Tone.C5);
            PlayNote(BuzzerClick.Tone.G4);
            PlayNote(BuzzerClick.Tone.E4);
            PlayNote(BuzzerClick.Tone.C4);

            _buzz.Play();

            Thread.Sleep(100);

            PlayNote(BuzzerClick.Tone.E4);
            PlayNote(BuzzerClick.Tone.G4);
            PlayNote(BuzzerClick.Tone.C5);
            PlayNote(BuzzerClick.Tone.G4);
            PlayNote(BuzzerClick.Tone.E4);
            PlayNote(BuzzerClick.Tone.C4);

            _buzz.Play();

            Thread.Sleep(5000);

            var melody = new BuzzerClick.Melody();

            // up
            melody.Add(BuzzerClick.Tone.C4, 200);
            melody.Add(BuzzerClick.Tone.D4, 200);
            melody.Add(BuzzerClick.Tone.E4, 200);
            melody.Add(BuzzerClick.Tone.F4, 200);
            melody.Add(BuzzerClick.Tone.G4, 200);
            melody.Add(BuzzerClick.Tone.A4, 200);
            melody.Add(BuzzerClick.Tone.B4, 200);
            melody.Add(BuzzerClick.Tone.C5, 200);
            melody.Add(BuzzerClick.Tone.B4, 200);
            melody.Add(BuzzerClick.Tone.A4, 200);
            melody.Add(BuzzerClick.Tone.G4, 200);
            melody.Add(BuzzerClick.Tone.F4, 200);
            melody.Add(BuzzerClick.Tone.E4, 200);
            melody.Add(BuzzerClick.Tone.D4, 200);
            melody.Add(BuzzerClick.Tone.C4, 200);
            melody.Add(BuzzerClick.Tone.E4, 200);
            melody.Add(BuzzerClick.Tone.G4, 200);
            melody.Add(BuzzerClick.Tone.C5, 200);
            melody.Add(BuzzerClick.Tone.G4, 200);
            melody.Add(BuzzerClick.Tone.E4, 200);
            melody.Add(BuzzerClick.Tone.C4, 200);

            _buzz.Play(melody);
        }

        static void PlayNote(BuzzerClick.Tone tone)
        {
            var note = new BuzzerClick.MusicNote(tone, 200);

            _buzz.AddNote(note);
        }
    }
}