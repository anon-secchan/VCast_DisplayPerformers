using BepInEx.Logging;
using UnityEngine;
using Infiniteloop.VRLive;
using System.Collections.Generic;
using System;
using UniRx;
using System.Threading.Tasks;
using System.Threading;

namespace DisplayPerformers
{
    public class PerformersState
    {
        private VCastModManager.VCastModManager modManager;
        [SerializeField] private Performers performers;
        private Studio _thisStudio;
        private IDisposable _performersOnJoinedDisposable;
        private IDisposable _performersOnLeftDisposable;

        public void Start()
        {
            BepInEx.Logger.Log(LogLevel.Message, "PerformersState Start()");
            modManager = new VCastModManager.VCastModManager(DisplayPerformers.GUID);
            Init();
        }

        public void Init()
        {
            if (modManager.IsModEnable)
            {
                try
                {
                    StudioChangeCheck();
                }
                catch (Exception ex)
                {
                    modManager.ErrorReport(ex);
                    Init();
                }
            }

        }

        public async Task StudioChangeCheck()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (_thisStudio != Studio.Current)
                    {
                        BepInEx.Logger.Log(LogLevel.Debug, "Studio.Current change detected!");
                        this._thisStudio = Studio.Current;
                        GetStudioCurrentAsync();

                    }
                    Thread.Sleep(500);
                }
            });

        }

        public async Task GetStudioCurrentAsync()
        {
            await Task.Run(() =>
            {
                while (Studio.Current == null)
                {
                    BepInEx.Logger.Log(LogLevel.Debug, "GetStudioCurrentAsync() while");
                    Thread.Sleep(1000);
                }

                BepInEx.Logger.Log(LogLevel.Debug, "Studio.Current get!");

                DisplayPerformers.DPerformers = new Dictionary<int, DisplayPerformers.DPerformer>();

                if (_performersOnJoinedDisposable != null)
                {
                    BepInEx.Logger.Log(LogLevel.Debug, "_performersOnJoinedDisposable.Dispose();");
                    _performersOnJoinedDisposable.Dispose();
                }

                if (_performersOnLeftDisposable != null)
                {
                    BepInEx.Logger.Log(LogLevel.Debug, "_performersOnLeftDisposable.Dispose();");
                    _performersOnLeftDisposable.Dispose();
                }

                performers = _thisStudio.Performers;

                foreach (var performer in performers)
                {
                    AddPerformer(performer);
                }

                PerformersSubscribe();
            });
        }


        public void PerformersSubscribe()
        {
            _performersOnJoinedDisposable = performers.OnJoined.Subscribe(performer =>
            {
                BepInEx.Logger.Log(LogLevel.Debug, "performers.OnJoined.Subscribe");
                AddPerformer(performer);

            });

            _performersOnLeftDisposable = performers.OnLeft.Subscribe(performer =>
            {
                BepInEx.Logger.Log(LogLevel.Debug, "performers.OnLeft.Subscribe");
                DisplayPerformers.DPerformers.Remove(performer.ID);
            });

        }

        public void AddPerformer(Performer performer)
        {
            DisplayPerformers.DPerformer dperformer = new DisplayPerformers.DPerformer();
            dperformer.Nickname = performer.Nickname;
            BepInEx.Logger.Log(LogLevel.Debug, $"Performer ID: {performer.ID}, DPerformer Nickname: {dperformer.Nickname}");
            DisplayPerformers.DPerformers[performer.ID] = dperformer;

            performer.HauntingCharacter.Subscribe(character =>
            {
                Thread.Sleep(10);
                if (character != null)
                {
                    character.InstantiatedModel.Subscribe(model =>
                    {
                        BepInEx.Logger.Log(LogLevel.Debug, "character.InstantiatedModel.Subscribe");
                        dperformer.thumb = model.Metadata.Thumbnail;
                    });
                }
            });

        }

    }
}
