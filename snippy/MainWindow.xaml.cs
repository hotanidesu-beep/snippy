using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace snippy
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private int _points = 0;
        private Random _random = new Random();
        private readonly string _saveFilePath;
        
        public ObservableCollection<TodoItem> Todos { get; set; } = new ObservableCollection<TodoItem>();

        // 5x7 Pixel Art Definitions (1 = filled, 0 = empty)
        private readonly Dictionary<char, int[]> _pixelFont = new Dictionary<char, int[]>
        {
            {'0', new int[] { 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1 }},
            {'1', new int[] { 0,0,1,0,0, 0,1,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,1,1,1,0 }},
            {'2', new int[] { 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1 }},
            {'3', new int[] { 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1 }},
            {'4', new int[] { 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 0,0,0,0,1 }},
            {'5', new int[] { 1,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1 }},
            {'6', new int[] { 1,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1 }},
            {'7', new int[] { 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 0,0,0,1,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0 }},
            {'8', new int[] { 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1 }},
            {'9', new int[] { 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1 }},
            {':', new int[] { 0,0,0,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,0,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,0,0,0 }},
            {'/', new int[] { 0,0,0,0,1, 0,0,0,0,1, 0,0,0,1,0, 0,0,1,0,0, 0,1,0,0,0, 1,0,0,0,0, 1,0,0,0,0 }},
            {' ', new int[] { 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0 }},
            {'P', new int[] { 1,1,1,1,0, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0 }},
            {'o', new int[] { 0,0,0,0,0, 0,0,0,0,0, 0,1,1,1,0, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 0,1,1,1,0 }},
            {'i', new int[] { 0,0,1,0,0, 0,0,0,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0 }},
            {'n', new int[] { 0,0,0,0,0, 0,0,0,0,0, 1,1,1,1,0, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1 }},
            {'t', new int[] { 0,0,1,0,0, 0,1,1,1,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,0,1,1 }},
            {'s', new int[] { 0,0,0,0,0, 0,0,0,0,0, 0,1,1,1,1, 1,0,0,0,0, 0,1,1,1,0, 0,0,0,0,1, 1,1,1,1,0 }},
            {'L', new int[] { 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1 }},
            {'K', new int[] { 1,0,0,0,1, 1,0,0,1,0, 1,0,1,0,0, 1,1,0,0,0, 1,0,1,0,0, 1,0,0,1,0, 1,0,0,0,1 }},
            {'C', new int[] { 0,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0, 0,1,1,1,1 }},
            {'G', new int[] { 0,1,1,1,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 0,1,1,1,0 }},
            {'S', new int[] { 0,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 0,1,1,1,0, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,0 }},
            {'*', new int[] { 0,0,0,0,0, 1,0,1,0,1, 0,1,1,1,0, 0,0,1,0,0, 0,1,1,1,0, 1,0,1,0,1, 0,0,0,0,0 }},
            {'x', new int[] { 0,0,0,0,0, 0,0,0,0,0, 1,0,0,0,1, 0,1,0,1,0, 0,0,1,0,0, 0,1,0,1,0, 1,0,0,0,1 }}
        };

        private double _lastKnobAngle = 0;
        private double _accumulatedKnobAngle = 0;
        
        // ラックチェック State
        private int _slotDigit1 = 0;
        private int _slotDigit2 = 0;
        private int _slotDigit3 = 0;
        private int _finalDigit1 = 0;
        private int _finalDigit2 = 0;
        private int _finalDigit3 = 0;
        private int _spinSlowdownCounter = 0;
        private bool _isSpinningSlot = false;
        private DispatcherTimer _slotSpinTimer;
        private DateTime _slotSpinStartTime;

        // Sticky Note Dragging State
        private bool _isDraggingNote = false;
        private Border _draggingNote = null;
        private Point _dragStartMousePos;
        private Point _dragStartNotePos;

        // Lucky Mode State
        private bool _isLuckyMode = false;
        private int _luckyRemainingSpins = 0;
        
        // Pity/Ceiling System
        private int _totalSpinsCounter = 0;

        // Customization State
        private string _tvFrameColor = "#333333";
        private string _screenGridColor = "#FF0D3A20";
        private double _tvOuterRadius = 8.0;
        private double _tvInnerRadius = 8.0;

        // Slime Creature System
        private int _slimeLevel = 0;
        private string _slimeMood = "neutral"; // neutral, happy, jackpot, sad
        private DispatcherTimer _slimeMoodTimer;
        private DispatcherTimer _slimeIdleTimer;

        // ═══ Roguelike System ═══
        // Combo streak
        private int _comboStreak = 0;
        // Relics
        private List<string> _ownedRelics = new List<string>();
        private int _totalWinCount = 0;
        private static readonly string[] ALL_RELICS = new[]
        {
            "GOLDEN_FINGER",  // 勝利ボーナス+50%
            "LUCKY_CHARM",    // ラッキーモード確率+10%
            "THRIFTY_WALLET", // スピンコスト-2
            "DIVINE_FAVOR",   // 天井75回
            "CHAIN_PROOF",    // コンボ倍率+50%
            "MAGIC_DICE",     // 10%でスピン無料
            "DOUBLE_UP",      // 15%でボーナス2倍
            "PITY_GUARD",     // 0pt時15pt復活
        };
        // Skills
        private bool _skillGuardActive = false;  // 次スピン無料
        private bool _skillSurgeActive = false;  // 次の勝利2倍
        // Spin modifiers
        private int _currentSpinCost = 10;
        private string _pendingSpinEvent = null; // "blessed" or "cursed"

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up ラックチェック timer
            _slotSpinTimer = new DispatcherTimer();
            _slotSpinTimer.Interval = TimeSpan.FromMilliseconds(50); // fast update for "spinning" effect
            _slotSpinTimer.Tick += SlotSpinTimer_Tick;
            
            // Set up save file path in MyDocuments
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolderPath = System.IO.Path.Combine(docPath, "SnippyClock");
            if (!Directory.Exists(appFolderPath))
                Directory.CreateDirectory(appFolderPath);
            _saveFilePath = System.IO.Path.Combine(appFolderPath, "save.json");

            lstTodos.ItemsSource = Todos;

            // Start clock
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Initial update
            Timer_Tick(null, null);

            // Setup Knob Event
            Knob1.AngleChangedEvent += Knob1_AngleChanged;

            // Start CRT Scanline Animation
            this.Loaded += (s, e) => {
                var anim = (Storyboard)this.Resources["ScanlineAnim"];
                anim.Begin();
                LoadData();
                UpdatePointsDisplay();
                UpdateSlotDisplay();
                UpdateLuckyDisplay();
                UpdateComboDisplay();
                UpdateSkillDisplay();
                UpdateRelicDisplay();
                ApplyCustomizations();
                UpdateSlimeDisplay();
                StartTodoButtonPulse();
                StartSlimeIdleAnimation();
                SetupSkillContextMenu();
            };
        }

        private void Knob1_AngleChanged(object sender, double newAngle)
        {
            if (_isSpinningSlot)
            {
                // Ignore knob while ラックチェック is spinning
                _lastKnobAngle = newAngle;
                return;
            }

            // Calculate angle delta
            double delta = newAngle - _lastKnobAngle;
            
            // Handle angle wrap-around (e.g. 350 to 10 -> delta = -340, actual should be +20)
            if (delta > 180) delta -= 360;
            else if (delta < -180) delta += 360;

            _lastKnobAngle = newAngle;
            _accumulatedKnobAngle += Math.Abs(delta);

            if (_accumulatedKnobAngle >= 360)
            {
                _accumulatedKnobAngle -= 360;
                StartSlotSpin();
            }
        }

        private void StartSlotSpin()
        {
            // 今スピンのコスト計算（レリック・スキルで変動）
            _currentSpinCost = 10;
            if (_ownedRelics.Contains("THRIFTY_WALLET"))
                _currentSpinCost = Math.Max(5, _currentSpinCost - 2);
            if (_skillGuardActive)
            {
                _currentSpinCost = 0;
                _skillGuardActive = false;
                UpdateSkillDisplay();
            }
            else if (_ownedRelics.Contains("MAGIC_DICE") && _random.NextDouble() < 0.10)
            {
                _currentSpinCost = 0;
                ShowEventNotification("✦ 魔法のサイコロ発動！\n無料スピン！");
            }

            // ランダムスピンイベント（6%）
            if (_pendingSpinEvent == null && _random.NextDouble() < 0.06)
            {
                _pendingSpinEvent = _random.NextDouble() < 0.5 ? "blessed" : "cursed";
                if (_pendingSpinEvent == "blessed")
                {
                    _currentSpinCost = 0;
                    ShowEventNotification("★ 祝福のスピン！\n無料で回せる！");
                }
                else
                {
                    ShowEventNotification("☠ 呪いのスピン...\n勝てば+75%ボーナス！");
                }
            }

            if (_points >= _currentSpinCost)
            {
                _isSpinningSlot = true;
                _slotSpinStartTime = DateTime.Now;
                _spinSlowdownCounter = 0;
                CalculateFinalOutcome();
                _slotSpinTimer.Start();
            }
            else
            {
                _accumulatedKnobAngle = 0;
                PlayInsufficientPointsAnimation();
            }
        }

        private void CalculateFinalOutcome()
        {
            _totalSpinsCounter++;

            _finalDigit1 = _random.Next(0, 10);
            _finalDigit2 = _random.Next(0, 10);
            _finalDigit3 = _random.Next(0, 10);

            // ワイルドシンボル★（-1）: 5勝以降、各スロット4%で出現
            if (_totalWinCount >= 5)
            {
                if (_random.NextDouble() < 0.04) _finalDigit1 = -1;
                if (_random.NextDouble() < 0.04) _finalDigit2 = -1;
                if (_random.NextDouble() < 0.04) _finalDigit3 = -1;
            }

            // 天井システム: DIVINE_FAVORがあれば75回、なければ100回
            int ceiling = _ownedRelics.Contains("DIVINE_FAVOR") ? 75 : 100;
            if (_totalSpinsCounter % ceiling == 0)
            {
                if (_random.NextDouble() < 0.90)
                {
                    int d = _random.Next(0, 10);
                    _finalDigit1 = _finalDigit2 = _finalDigit3 = d;
                }
            }
            // ラッキーモード中: LUCKY_CHARMで+10%
            else if (_isLuckyMode)
            {
                double luckyChance = _ownedRelics.Contains("LUCKY_CHARM") ? 0.40 : 0.30;
                if (_random.NextDouble() < luckyChance)
                {
                    int d = _random.Next(0, 10);
                    _finalDigit1 = _finalDigit2 = _finalDigit3 = d;
                }
            }
        }

        // ワイルド（-1）は任意の数字に一致
        private static bool SlotMatches(int a, int b) => a == -1 || b == -1 || a == b;
        private static char SlotChar(int d) => d == -1 ? '*' : (char)('0' + d);

        private void SlotSpinTimer_Tick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - _slotSpinStartTime).TotalSeconds;

            if (elapsed < 0.7)
            {
                // Phase 1: 全桁高速スピン
                _slotDigit1 = _random.Next(0, 10);
                _slotDigit2 = _random.Next(0, 10);
                _slotDigit3 = _random.Next(0, 10);
            }
            else if (elapsed < 1.2)
            {
                // Phase 2: 1桁目ロック、2・3桁目スピン
                _slotDigit1 = _finalDigit1;
                _slotDigit2 = _random.Next(0, 10);
                _slotDigit3 = _random.Next(0, 10);
            }
            else if (elapsed < 1.65)
            {
                // Phase 3: 1・2桁目ロック、3桁目スピン
                _slotDigit1 = _finalDigit1;
                _slotDigit2 = _finalDigit2;
                _slotDigit3 = _random.Next(0, 10);
            }
            else if (elapsed < 2.1)
            {
                // Phase 4: 3桁目スローダウン (徐々に間引く)
                _slotDigit1 = _finalDigit1;
                _slotDigit2 = _finalDigit2;
                double phaseProgress = (elapsed - 1.65) / 0.45; // 0→1
                int skipRate = (int)(phaseProgress * 5) + 1;     // 1→6
                _spinSlowdownCounter++;
                if (_spinSlowdownCounter >= skipRate)
                {
                    _slotDigit3 = _random.Next(0, 10);
                    _spinSlowdownCounter = 0;
                }
            }
            else
            {
                // Phase 5: 全桁停止 → 結果処理
                _slotDigit1 = _finalDigit1;
                _slotDigit2 = _finalDigit2;
                _slotDigit3 = _finalDigit3;
                _slotSpinTimer.Stop();
                FinishSlotSpin();
                return;
            }

            UpdateSlotDisplay(true);
        }

        private void FinishSlotSpin()
        {
            int cost = _currentSpinCost;
            int bonus = 0;
            bool isWin = false;
            bool isHalfWin = false;

            // ワイルド対応の勝利判定
            bool allMatch = SlotMatches(_finalDigit1, _finalDigit2)
                         && SlotMatches(_finalDigit2, _finalDigit3)
                         && SlotMatches(_finalDigit1, _finalDigit3);
            bool twoMatch = !allMatch && (SlotMatches(_finalDigit1, _finalDigit2)
                                       || SlotMatches(_finalDigit2, _finalDigit3)
                                       || SlotMatches(_finalDigit1, _finalDigit3));

            // ジャックポット判定用の実際の数字（ワイルド除く）
            int effectiveDigit = _finalDigit1 != -1 ? _finalDigit1
                               : _finalDigit2 != -1 ? _finalDigit2 : _finalDigit3;
            bool isJackpot = allMatch && effectiveDigit == 7;

            if (allMatch)
            {
                isWin = true;
                cost = 0;
                bonus = isJackpot ? 100 : 20;

                // レリック: GOLDEN_FINGER - ボーナス+50%
                if (_ownedRelics.Contains("GOLDEN_FINGER"))
                    bonus = (int)(bonus * 1.5);
                // レリック: DOUBLE_UP - 15%でボーナス2倍
                if (_ownedRelics.Contains("DOUBLE_UP") && _random.NextDouble() < 0.15)
                {
                    bonus *= 2;
                    ShowEventNotification("✦ ダブルアップ発動！\nボーナス2倍！");
                }
                // スキル: SURGE - 2倍
                if (_skillSurgeActive)
                {
                    bonus *= 2;
                    _skillSurgeActive = false;
                    UpdateSkillDisplay();
                    ShowEventNotification("⚡ 倍増発動！\nボーナス2倍！");
                }
                // イベント: 呪いスピン - 勝利ボーナス+75%
                if (_pendingSpinEvent == "cursed")
                    bonus = (int)(bonus * 1.75);
                _pendingSpinEvent = null;

                // コンボ乗数
                _comboStreak++;
                double comboMult = GetComboMultiplier();
                if (_ownedRelics.Contains("CHAIN_PROOF"))
                    comboMult = 1.0 + (comboMult - 1.0) * 1.5;
                bonus = (int)(bonus * comboMult);
                if (_comboStreak >= 2)
                    ShowEventNotification($"🔥 コンボ x{_comboStreak}！\nボーナス x{comboMult:0.0}");

                _totalWinCount++;
                if (!_isLuckyMode)
                {
                    _isLuckyMode = true;
                    _luckyRemainingSpins = 10;
                }
                _slimeMood = isJackpot ? "jackpot" : "happy";
                SetSlimeMoodTimer(isJackpot ? 3000 : 2000);
            }
            else if (twoMatch)
            {
                isHalfWin = true;
                cost = 0;
                _comboStreak = 0;
                _pendingSpinEvent = null;
                _slimeMood = "neutral";
            }
            else
            {
                _comboStreak = 0;
                _pendingSpinEvent = null;
                _slimeMood = "sad";
                SetSlimeMoodTimer(1500);
            }

            // ラッキーモード残回数カウントダウン
            if (_isLuckyMode)
            {
                _luckyRemainingSpins--;
                if (_luckyRemainingSpins <= 0)
                {
                    if (_random.NextDouble() < 0.30)
                    {
                        _luckyRemainingSpins = 10;
                        PlayLuckyContinueAnimation();
                    }
                    else
                    {
                        _isLuckyMode = false;
                    }
                }
            }

            _points -= cost;

            // レリック: PITY_GUARD - 0pt以下なら15ptで復活
            if (_points <= 0 && _ownedRelics.Contains("PITY_GUARD"))
            {
                _points = 15;
                ShowEventNotification("🛡 加護の盾発動！\n15pt復活！");
            }

            if (bonus > 0) AddPoints(bonus);

            UpdatePointsDisplay();
            UpdateSlotDisplay(false, isWin, isHalfWin);
            UpdateLuckyDisplay();
            UpdateComboDisplay();

            _slimeLevel++;
            UpdateSlimeDisplay();

            // 5勝ごとにレリック付与
            if (isWin && _totalWinCount > 0 && _totalWinCount % 5 == 0)
                GrantRandomRelic();

            if (isJackpot)
                PlayJackpotAnimation();
            else if (isWin)
                PlayWinAnimation();
            else if (isHalfWin)
                PlayHalfWinAnimation();
            else
            {
                ApplyRandomMinorChange();
                PlayFeedbackAnimation();
            }

            SaveData();
            _isSpinningSlot = false;
        }

        private void UpdateSlotDisplay(bool isSpinning = false, bool isWin = false, bool isHalfWin = false)
        {
            if (pixelCanvasSlot != null)
            {
                string slotText = $"{SlotChar(_slotDigit1)} {SlotChar(_slotDigit2)} {SlotChar(_slotDigit3)}";
                Color color = Colors.White;

                if (!isSpinning)
                {
                    if (isWin)           color = Colors.Gold;
                    else if (isHalfWin)  color = Color.FromArgb(255, 255, 165, 0); // オレンジ
                    else                 color = Color.FromArgb(255, 200, 200, 200);
                }

                DrawPixelText(pixelCanvasSlot, slotText, 1, new SolidColorBrush(color));

                if (pixelCanvasSlot.Effect is DropShadowEffect shadow)
                {
                    if (isWin)          shadow.Color = Colors.Gold;
                    else if (isHalfWin) shadow.Color = Color.FromArgb(255, 255, 165, 0);
                    else                shadow.Color = Colors.White;
                }
            }
        }

        private void UpdateLuckyDisplay()
        {
            if (pixelCanvasLucky == null) return;
            if (_isLuckyMode)
            {
                DrawPixelText(pixelCanvasLucky, $"L:{_luckyRemainingSpins}", 1,
                    new SolidColorBrush(Color.FromArgb(255, 255, 100, 100)));
                pixelCanvasLucky.Visibility = Visibility.Visible;
            }
            else
            {
                pixelCanvasLucky.Children.Clear();
                pixelCanvasLucky.Visibility = Visibility.Hidden;
            }
        }

        private void PlayInsufficientPointsAnimation()
        {
            // ポイント不足: 赤フラッシュのみ
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Red;
                    flashAnim.Begin();
                    var reset = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    reset.Tick += (s, e) => { FlashOverlay.Fill = Brushes.White; reset.Stop(); };
                    reset.Start();
                }
            }
            catch { }
        }

        private void PlayHalfWinAnimation()
        {
            // 2桁一致: オレンジフラッシュ
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 165, 0));
                    flashAnim.Begin();
                    var reset = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    reset.Tick += (s, e) => { FlashOverlay.Fill = Brushes.White; reset.Stop(); };
                    reset.Start();
                }
            }
            catch { }
        }

        private void PlayJackpotAnimation()
        {
            // 777ジャックポット: 大きいスケール + 複数シェイク + 画面背景ゴールド
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Gold;
                    flashAnim.Begin();
                    var reset = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    reset.Tick += (s, e) => { FlashOverlay.Fill = Brushes.White; reset.Stop(); };
                    reset.Start();
                }

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();
                var shake2 = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
                shake2.Tick += (s, e) => { shakeAnim?.Begin(); shake2.Stop(); };
                shake2.Start();
                var shake3 = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
                shake3.Tick += (s, e) => { shakeAnim?.Begin(); shake3.Stop(); };
                shake3.Start();

                if (pixelCanvasSlot != null)
                {
                    var st = new ScaleTransform(1, 1);
                    pixelCanvasSlot.RenderTransform = st;
                    pixelCanvasSlot.RenderTransformOrigin = new Point(0.5, 0.5);
                    var animX = new DoubleAnimation(1, 2.5, TimeSpan.FromMilliseconds(150)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(5) };
                    var animY = new DoubleAnimation(1, 2.5, TimeSpan.FromMilliseconds(150)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(5) };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
                }

                // 画面背景をゴールドに一時変更
                if (ScreenGrid != null)
                {
                    var origColor = _screenGridColor;
                    ScreenGrid.Background = new SolidColorBrush(Color.FromArgb(255, 160, 130, 0));
                    var bgTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
                    bgTimer.Tick += (s, e) =>
                    {
                        ScreenGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(origColor));
                        bgTimer.Stop();
                    };
                    bgTimer.Start();
                }
            }
            catch { }
        }

        private void PlayLuckyContinueAnimation()
        {
            // 幸運継続時の演出（画面が全体的に赤く光るなど）
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Red;
                    flashAnim.Begin();
                    
                    var resetTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    resetTimer.Tick += (s, e) => {
                        FlashOverlay.Fill = Brushes.White;
                        resetTimer.Stop();
                    };
                    resetTimer.Start();
                }

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();
            }
            catch { }
        }

        private void PlayWinAnimation()
        {
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Gold;
                    flashAnim.Begin();
                    
                    var resetTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    resetTimer.Tick += (s, e) => {
                        FlashOverlay.Fill = Brushes.White;
                        resetTimer.Stop();
                    };
                    resetTimer.Start();
                }

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();

                if (pixelCanvasSlot != null)
                {
                    var st = new ScaleTransform(1, 1);
                    pixelCanvasSlot.RenderTransform = st;
                    pixelCanvasSlot.RenderTransformOrigin = new Point(0.5, 0.5);
                    
                    var animX = new DoubleAnimation(1, 1.5, TimeSpan.FromMilliseconds(200)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) };
                    var animY = new DoubleAnimation(1, 1.5, TimeSpan.FromMilliseconds(200)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
                }
            }
            catch { }
        }

        private void PlayFeedbackAnimation()
        {
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                flashAnim?.Begin();

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();
            }
            catch { }
        }

        private void ApplyRandomMinorChange()
        {
            int choice = _random.Next(4);
            switch(choice)
            {
                case 0: // Change TV Frame Color slightly
                    _tvFrameColor = TweakHexColor(_tvFrameColor);
                    break;
                case 1: // Change Screen Grid Color slightly
                    _screenGridColor = TweakHexColor(_screenGridColor);
                    break;
                case 2: // Change Outer Radius slightly
                    _tvOuterRadius = Math.Max(0, Math.Min(25, _tvOuterRadius + (_random.NextDouble() * 2 - 1))); // +/- 1
                    break;
                case 3: // Change Inner Radius slightly
                    _tvInnerRadius = Math.Max(0, Math.Min(15, _tvInnerRadius + (_random.NextDouble() * 2 - 1))); // +/- 1
                    break;
            }
            ApplyCustomizations();
        }

        private string TweakHexColor(string hex)
        {
            try
            {
                Color c = (Color)ColorConverter.ConvertFromString(hex);
                // Adjust RGB each by -10 to +10 randomly
                byte r = (byte)Math.Max(0, Math.Min(255, c.R + _random.Next(-10, 11)));
                byte g = (byte)Math.Max(0, Math.Min(255, c.G + _random.Next(-10, 11)));
                byte b = (byte)Math.Max(0, Math.Min(255, c.B + _random.Next(-10, 11)));
                return $"#{(c.A):X2}{r:X2}{g:X2}{b:X2}";
            }
            catch { return hex; }
        }

        private void ApplyCustomizations()
        {
            try
            {
                if (TvBodyPath != null)
                    TvBodyPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_tvFrameColor));
                
                if (ScreenGrid != null)
                    ScreenGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_screenGridColor));
                
                if (TvOuterBox != null)
                {
                    TvOuterBox.RadiusX = _tvOuterRadius;
                    TvOuterBox.RadiusY = _tvOuterRadius;
                }

                if (TvInnerBox != null)
                {
                    TvInnerBox.RadiusX = _tvInnerRadius;
                    TvInnerBox.RadiusY = _tvInnerRadius;
                }
            }
            catch { }
        }

        // ─── スライムシステム ─────────────────────────────────
        private void UpdateSlimeDisplay()
        {
            if (pixelCanvasSlime == null) return;
            DrawSlimePixelArt(_slimeLevel, _slimeMood);
            GenerateParticles();
        }

        private void DrawSlimePixelArt(int level, string mood)
        {
            if (pixelCanvasSlime == null) return;
            pixelCanvasSlime.Children.Clear();

            // ステージ: レベルに応じて5段階
            int stage = level < 3 ? 0 : level < 10 ? 1 : level < 20 ? 2 : level < 35 ? 3 : 4;

            double cw = pixelCanvasSlime.Width > 0 ? pixelCanvasSlime.Width : 35;
            double ch = pixelCanvasSlime.Height > 0 ? pixelCanvasSlime.Height : 35;
            double cx = cw / 2.0;

            // ボディサイズ（ステージで成長）
            double bw = 10 + stage * 5;
            double bh = 8 + stage * 3.5;

            // 気分・ステージで色変化
            Color bodyColor = mood switch
            {
                "happy"   => Color.FromArgb(255, 255, 215,  0),
                "jackpot" => Color.FromArgb(255, 255, 100, 50),
                "sad"     => Color.FromArgb(255, 140,  80, 200),
                _ => stage switch
                {
                    0 => Color.FromArgb(255,  80, 160, 220),   // ベビー：水色
                    1 => Color.FromArgb(255,  70, 200, 120),   // スライム：緑
                    2 => Color.FromArgb(255,  60, 130, 230),   // 成長：青
                    3 => Color.FromArgb(255, 240, 120,  40),   // 進化：オレンジ
                    _ => Color.FromArgb(255, 230,  60, 150)    // キング：ピンク
                }
            };
            Color shadowColor = Color.FromArgb(160,
                (byte)(bodyColor.R * 0.55), (byte)(bodyColor.G * 0.55), (byte)(bodyColor.B * 0.55));

            // 王冠（ステージ3以上）
            if (stage >= 3)
            {
                double crownW = bw * 0.75;
                double crownTop = ch - bh - bh * 0.6 - 6;
                var crownBase = new Rectangle { Width = crownW, Height = 3,
                    Fill = new SolidColorBrush(Colors.Gold) };
                Canvas.SetLeft(crownBase, cx - crownW / 2);
                Canvas.SetTop(crownBase, crownTop + 3);
                pixelCanvasSlime.Children.Add(crownBase);
                for (int i = 0; i < 3; i++)
                {
                    var pt = new Rectangle { Width = 3, Height = 4,
                        Fill = new SolidColorBrush(stage >= 4 ? Colors.Gold : Colors.Goldenrod) };
                    Canvas.SetLeft(pt, cx - crownW / 2 + i * crownW / 2.5 + 1);
                    Canvas.SetTop(pt, crownTop);
                    pixelCanvasSlime.Children.Add(pt);
                }
            }

            // 上のコブ（ぷよっとした頭）
            double bumpW = bw * 0.55;
            double bumpH = bh * 0.65;
            double bodyTop = ch - bh - 2;
            var bump = new Ellipse { Width = bumpW, Height = bumpH,
                Fill = new SolidColorBrush(bodyColor) };
            Canvas.SetLeft(bump, cx - bumpW / 2);
            Canvas.SetTop(bump, bodyTop - bumpH * 0.65);
            pixelCanvasSlime.Children.Add(bump);

            // メインボディ
            var body = new Ellipse { Width = bw, Height = bh,
                Fill = new SolidColorBrush(bodyColor) };
            Canvas.SetLeft(body, cx - bw / 2);
            Canvas.SetTop(body, bodyTop);
            pixelCanvasSlime.Children.Add(body);

            // 影（底の楕円）
            var shadow = new Ellipse { Width = bw * 0.7, Height = bh * 0.22,
                Fill = new SolidColorBrush(shadowColor) };
            Canvas.SetLeft(shadow, cx - bw * 0.35);
            Canvas.SetTop(shadow, bodyTop + bh * 0.8);
            pixelCanvasSlime.Children.Add(shadow);

            // 目の位置
            double eyeSz = stage >= 2 ? 2.5 : 2.0;
            double eyeY  = bodyTop + bh * 0.28;
            double eyeGap = bw * 0.22;

            // ステージ2以上：目の白
            if (stage >= 2)
            {
                var lw = new Ellipse { Width = eyeSz + 1.5, Height = eyeSz + 1.5, Fill = Brushes.White };
                Canvas.SetLeft(lw, cx - eyeGap - (eyeSz + 1.5) / 2);
                Canvas.SetTop(lw, eyeY - 0.5);
                pixelCanvasSlime.Children.Add(lw);
                var rw = new Ellipse { Width = eyeSz + 1.5, Height = eyeSz + 1.5, Fill = Brushes.White };
                Canvas.SetLeft(rw, cx + eyeGap - (eyeSz + 1.5) / 2);
                Canvas.SetTop(rw, eyeY - 0.5);
                pixelCanvasSlime.Children.Add(rw);
            }
            var le = new Ellipse { Width = eyeSz, Height = eyeSz, Fill = Brushes.Black };
            Canvas.SetLeft(le, cx - eyeGap - eyeSz / 2);
            Canvas.SetTop(le, eyeY);
            pixelCanvasSlime.Children.Add(le);
            var re = new Ellipse { Width = eyeSz, Height = eyeSz, Fill = Brushes.Black };
            Canvas.SetLeft(re, cx + eyeGap - eyeSz / 2);
            Canvas.SetTop(re, eyeY);
            pixelCanvasSlime.Children.Add(re);

            // 口の表情
            double mouthY = eyeY + eyeSz + 2;
            if (mood == "sad")
            {
                // 逆ハの字 (しょんぼり)
                var ml = new Rectangle { Width = 3, Height = 1, Fill = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5) };
                ml.RenderTransform = new RotateTransform(-25);
                Canvas.SetLeft(ml, cx - 4); Canvas.SetTop(ml, mouthY + 1);
                pixelCanvasSlime.Children.Add(ml);
                var mr = new Rectangle { Width = 3, Height = 1, Fill = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5) };
                mr.RenderTransform = new RotateTransform(25);
                Canvas.SetLeft(mr, cx + 1); Canvas.SetTop(mr, mouthY + 1);
                pixelCanvasSlime.Children.Add(mr);
            }
            else if (stage >= 1 || mood == "happy" || mood == "jackpot")
            {
                // ハの字 (笑顔)
                var ml = new Rectangle { Width = 3, Height = 1, Fill = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5) };
                ml.RenderTransform = new RotateTransform(25);
                Canvas.SetLeft(ml, cx - 4); Canvas.SetTop(ml, mouthY);
                pixelCanvasSlime.Children.Add(ml);
                var mr = new Rectangle { Width = 3, Height = 1, Fill = Brushes.Black,
                    RenderTransformOrigin = new Point(0.5, 0.5) };
                mr.RenderTransform = new RotateTransform(-25);
                Canvas.SetLeft(mr, cx + 1); Canvas.SetTop(mr, mouthY);
                pixelCanvasSlime.Children.Add(mr);
            }

            // ほっぺ（ステージ2以上）
            if (stage >= 2)
            {
                var blush = Color.FromArgb(100, 255, 100, 100);
                var bl = new Ellipse { Width = 3.5, Height = 2.5, Fill = new SolidColorBrush(blush) };
                Canvas.SetLeft(bl, cx - eyeGap - 3.5);
                Canvas.SetTop(bl, eyeY + eyeSz + 0.5);
                pixelCanvasSlime.Children.Add(bl);
                var br = new Ellipse { Width = 3.5, Height = 2.5, Fill = new SolidColorBrush(blush) };
                Canvas.SetLeft(br, cx + eyeGap);
                Canvas.SetTop(br, eyeY + eyeSz + 0.5);
                pixelCanvasSlime.Children.Add(br);
            }

            // キングスライム（ステージ4）: 宝石のアクセント
            if (stage >= 4)
            {
                var gem = new Ellipse { Width = 4, Height = 4,
                    Fill = new SolidColorBrush(Colors.DeepSkyBlue) };
                Canvas.SetLeft(gem, cx - 2);
                Canvas.SetTop(gem, bodyTop - bumpH * 0.6 - 2);
                pixelCanvasSlime.Children.Add(gem);
            }

            // ハイライト（ツヤ感）
            var shine = new Ellipse { Width = 2.5, Height = 2,
                Fill = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)) };
            Canvas.SetLeft(shine, cx - bumpW * 0.18);
            Canvas.SetTop(shine, bodyTop - bumpH * 0.5);
            pixelCanvasSlime.Children.Add(shine);

            // ウィグルアニメーション
            AnimateSlimeWobble();
        }

        private void AnimateSlimeWobble()
        {
            try
            {
                if (pixelCanvasSlime?.Children.Count == 0) return;
                var st = new ScaleTransform(1, 1);
                pixelCanvasSlime.RenderTransform = st;
                pixelCanvasSlime.RenderTransformOrigin = new Point(0.5, 1.0);
                var keyY = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(450) };
                keyY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0,  TimeSpan.Zero));
                keyY.KeyFrames.Add(new LinearDoubleKeyFrame(1.25, TimeSpan.FromMilliseconds(100)));
                keyY.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, TimeSpan.FromMilliseconds(200)));
                keyY.KeyFrames.Add(new LinearDoubleKeyFrame(1.1,  TimeSpan.FromMilliseconds(320)));
                keyY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0,  TimeSpan.FromMilliseconds(450)));
                var keyX = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(450) };
                keyX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0,  TimeSpan.Zero));
                keyX.KeyFrames.Add(new LinearDoubleKeyFrame(0.82, TimeSpan.FromMilliseconds(100)));
                keyX.KeyFrames.Add(new LinearDoubleKeyFrame(1.15, TimeSpan.FromMilliseconds(200)));
                keyX.KeyFrames.Add(new LinearDoubleKeyFrame(0.95, TimeSpan.FromMilliseconds(320)));
                keyX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0,  TimeSpan.FromMilliseconds(450)));
                st.BeginAnimation(ScaleTransform.ScaleYProperty, keyY);
                st.BeginAnimation(ScaleTransform.ScaleXProperty, keyX);
            }
            catch { }
        }

        private void SetSlimeMoodTimer(int ms)
        {
            _slimeMoodTimer?.Stop();
            _slimeMoodTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            _slimeMoodTimer.Tick += (s, e) =>
            {
                _slimeMood = "neutral";
                _slimeMoodTimer.Stop();
                UpdateSlimeDisplay();
            };
            _slimeMoodTimer.Start();
        }

        private void StartSlimeIdleAnimation()
        {
            _slimeIdleTimer?.Stop();
            _slimeIdleTimer = new DispatcherTimer
                { Interval = TimeSpan.FromMilliseconds(_random.Next(3000, 6000)) };
            _slimeIdleTimer.Tick += (s, e) =>
            {
                _slimeIdleTimer.Stop();
                AnimateSlimeWobble();
                _slimeIdleTimer.Interval = TimeSpan.FromMilliseconds(_random.Next(3000, 6000));
                _slimeIdleTimer.Start();
            };
            _slimeIdleTimer.Start();
        }

        private void GenerateParticles()
        {
            if (EffectCanvas == null) return;
            
            EffectCanvas.Children.Clear();
            
            // Number of particles depends on slime level
            int particleCount = Math.Min(_slimeLevel * 3, 50);
            
            for (int i = 0; i < particleCount; i++)
            {
                Ellipse particle = new Ellipse
                {
                    Width = _random.Next(2, 5),
                    Height = _random.Next(2, 5),
                    Fill = new SolidColorBrush(Color.FromArgb((byte)_random.Next(100, 200), 255, 255, 200)),
                    Opacity = _random.NextDouble()
                };

                int canvasW = (int)(EffectCanvas.ActualWidth > 0 ? EffectCanvas.ActualWidth : 140);
                int canvasH = (int)(EffectCanvas.ActualHeight > 0 ? EffectCanvas.ActualHeight : 120);
                Canvas.SetLeft(particle, _random.Next(0, canvasW));
                Canvas.SetTop(particle, _random.Next(0, canvasH));
                
                EffectCanvas.Children.Add(particle);

                // Add simple animation
                DoubleAnimation anim = new DoubleAnimation
                {
                    From = Canvas.GetTop(particle),
                    To = Canvas.GetTop(particle) - _random.Next(10, 30),
                    Duration = TimeSpan.FromSeconds(_random.Next(2, 5)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                particle.BeginAnimation(Canvas.TopProperty, anim);
                
                DoubleAnimation opacityAnim = new DoubleAnimation
                {
                    From = particle.Opacity,
                    To = 0.1,
                    Duration = TimeSpan.FromSeconds(_random.Next(1, 3)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                particle.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            }
        }

        private void SaveData()
        {
            try
            {
                var notes = StickyNoteCanvas.Children.OfType<Border>()
                    .Where(b => b.Tag != null)
                    .Select(b => new SavedNote
                    {
                        Text = b.Tag.ToString(),
                        X = Canvas.GetLeft(b),
                        Y = Canvas.GetTop(b),
                        Angle = (b.RenderTransform as TransformGroup)?.Children.OfType<RotateTransform>().FirstOrDefault()?.Angle ?? 0,
                        Color = ((SolidColorBrush)b.Background).Color.ToString()
                    }).ToList();

                var data = new AppData
                {
                    Points = _points,
                    Notes = notes,
                    TvFrameColor = _tvFrameColor,
                    ScreenGridColor = _screenGridColor,
                    TvOuterRadius = _tvOuterRadius,
                    TvInnerRadius = _tvInnerRadius,
                    TotalSpins = _totalSpinsCounter,
                    SlimeLevel = _slimeLevel,
                    ComboStreak = _comboStreak,
                    OwnedRelics = _ownedRelics,
                    TotalWinCount = _totalWinCount,
                    SkillGuardActive = _skillGuardActive,
                    SkillSurgeActive = _skillSurgeActive,
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                // Optionally handle save errors
            }
        }

        private void LoadData()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    var data = JsonSerializer.Deserialize<AppData>(json);
                    
                    if (data != null)
                    {
                        _points = data.Points;
                        _tvFrameColor = data.TvFrameColor;
                        _screenGridColor = data.ScreenGridColor;
                        _tvOuterRadius = data.TvOuterRadius;
                        _tvInnerRadius = data.TvInnerRadius;
                        _slimeLevel = data.SlimeLevel;
                        _totalSpinsCounter = data.TotalSpins;
                        _comboStreak = data.ComboStreak;
                        _ownedRelics = data.OwnedRelics ?? new List<string>();
                        _totalWinCount = data.TotalWinCount;
                        _skillGuardActive = data.SkillGuardActive;
                        _skillSurgeActive = data.SkillSurgeActive;
                        
                        Todos.Clear(); // 起動時にTodoリストをクリア
                        foreach (var noteData in data.Notes)
                        {
                            RestoreStickyNote(noteData);
                            // 保存されている付箋を元にTodoメニューにも追加
                            Todos.Add(new TodoItem { Text = noteData.Text, IsCompleted = false });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Optionally handle load errors
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            string timeStr = now.ToString("HH:mm:ss");
            string dateStr = now.ToString("yyyy/MM/dd");
            
            if (now.Second % 2 == 0)
                timeStr = now.ToString("HH mm ss");

            if (pixelCanvasTime != null) DrawPixelText(pixelCanvasTime, timeStr, 2, new SolidColorBrush(Color.FromArgb(255, 74, 255, 112))); // #FF4AFF70
            if (pixelCanvasDate != null) DrawPixelText(pixelCanvasDate, dateStr, 1, new SolidColorBrush(Color.FromArgb(255, 74, 255, 112)));

            if (now.Second == 0 && !_isSpinningSlot)
                PlayMinuteChimeAnimation();
        }

        private void DrawPixelText(Canvas canvas, string text, int pixelSize, SolidColorBrush brush)
        {
            canvas.Children.Clear();
            
            int charWidth = 5;
            int charHeight = 7;
            int spacingX = 1;
            int currentX = 0;

            foreach (char c in text)
            {
                if (_pixelFont.ContainsKey(c))
                {
                    int[] pixels = _pixelFont[c];
                    for (int y = 0; y < charHeight; y++)
                    {
                        for (int x = 0; x < charWidth; x++)
                        {
                            int index = y * charWidth + x;
                            if (pixels[index] == 1)
                            {
                                Rectangle rect = new Rectangle
                                {
                                    Width = pixelSize,
                                    Height = pixelSize,
                                    Fill = brush
                                };
                                Canvas.SetLeft(rect, currentX + (x * pixelSize));
                                Canvas.SetTop(rect, y * pixelSize);
                                canvas.Children.Add(rect);
                            }
                        }
                    }
                }
                currentX += (charWidth + spacingX) * pixelSize;
            }
            canvas.Width = currentX;
            canvas.Height = charHeight * pixelSize;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Clear focus to allow any active TextBox to fire LostFocus and exit edit mode
            Keyboard.ClearFocus();

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnToggleTodo_Click(object sender, RoutedEventArgs e)
        {
            TodoAreaBorder.Visibility = TodoAreaBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (TodoAreaBorder.Visibility == Visibility.Visible)
            {
                this.Height = 600; // Expand to show todos + offset
            }
            else
            {
                this.Height = 400; // Collapse (Base is 400)
            }
        }

        private void BtnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTodoInput.Text))
            {
                string text = txtTodoInput.Text.Trim();

                // 隠しコマンド（デバッグ用）
                if (text.ToLower() == "god")
                {
                    AddPoints(1000);
                    SaveData();
                    txtTodoInput.Text = string.Empty;
                    return;
                }

                Todos.Add(new TodoItem { Text = text, IsCompleted = false });
                txtTodoInput.Text = string.Empty;
                AddStickyNote(text);
                SaveData();
            }
        }

        private void AddStickyNote(string text)
        {
            // Center of TV is around (150, 125) in Window coordinates
            // TV Frame bounds relative to Window approx: X: 55 to 245, Y: 55 to 195
            var note = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, (byte)_random.Next(200, 256), (byte)_random.Next(200, 256), (byte)_random.Next(150, 200))),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Width = 60,
                Height = 60,
                Padding = new Thickness(4),
                Tag = text // Store original text
            };

            var textBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Comic Sans MS"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            note.Child = textBlock;

            // Context Menu setup
            var contextMenu = new ContextMenu();
            
            var editMenuItem = new MenuItem { Header = "編集 (Edit)" };
            editMenuItem.Click += (s, e) => EditStickyNote(note, textBlock);
            
            var completeMenuItem = new MenuItem { Header = "達成 (Complete)" };
            completeMenuItem.Click += (s, e) => {
                double noteX = Canvas.GetLeft(note) + 30;
                double noteY = Canvas.GetTop(note) + 30;
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(text);
                AddPoints(10);
                PlayConfettiAnimation(noteX, noteY);
                SaveData();
            };

            var deleteMenuItem = new MenuItem { Header = "削除 (Delete)" };
            deleteMenuItem.Click += (s, e) => {
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(text);
                SaveData();
            };

            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(completeMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            note.ContextMenu = contextMenu;

            // Add drop shadow
            note.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 3,
                ShadowDepth = 2,
                Opacity = 0.5
            };

            // The TV Container is at Margin 100,100 and size is 200x150.
            // TV Bounds in Window: X = 100 to 300, Y = 100 to 250
            // Window Size: Width 400, Height 400 (or 600)
            // Sticky Note Size: Width 60, Height 60
            // We want sticky notes to be completely outside the TV bounds and not overlapping each other.

            double x = 0;
            double y = 0;
            double angle = 0;
            bool positionFound = false;
            int maxAttempts = 50;
            
            // Get existing notes bounds to prevent overlap
            var existingNotesRects = StickyNoteCanvas.Children.OfType<Border>()
                .Select(b => new Rect(Canvas.GetLeft(b), Canvas.GetTop(b), b.Width, b.Height))
                .ToList();

            for (int i = 0; i < maxAttempts; i++)
            {
                int edge = _random.Next(4);
                if (edge == 0) // Top
                {
                    x = _random.Next(20, 320);
                    y = _random.Next(10, 40);
                    angle = _random.Next(-25, 25);
                }
                else if (edge == 1) // Right
                {
                    x = _random.Next(310, 330);
                    y = _random.Next(50, 250);
                    angle = _random.Next(15, 45);
                }
                else if (edge == 2) // Bottom
                {
                    x = _random.Next(20, 320);
                    y = _random.Next(260, 300);
                    angle = _random.Next(-25, 25);
                }
                else // Left
                {
                    x = _random.Next(10, 40);
                    y = _random.Next(50, 250);
                    angle = _random.Next(-45, -15);
                }

                // Check collision with existing notes
                Rect newRect = new Rect(x, y, 60, 60);
                
                // Allow a little bit of overlap (e.g., shrink the collision rect slightly)
                Rect collisionRect = new Rect(x + 5, y + 5, 50, 50);
                
                bool isOverlapping = existingNotesRects.Any(r => r.IntersectsWith(collisionRect));

                if (!isOverlapping)
                {
                    positionFound = true;
                    break;
                }
            }

            // If we couldn't find a totally clear spot after 50 attempts, 
            // just use the last generated position (it will overlap, but screen is full)

            var rotateTransform = new RotateTransform(angle);
            var popScale = new ScaleTransform(0, 0);
            var transform = new TransformGroup();
            transform.Children.Add(rotateTransform);
            transform.Children.Add(popScale);
            note.RenderTransform = transform;
            note.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(note, x);
            Canvas.SetTop(note, y);

            // Re-enable hit test so user can click/right-click it
            note.IsHitTestVisible = true;

            // Register Drag and Drop events
            note.MouseLeftButtonDown += StickyNote_MouseLeftButtonDown;
            note.MouseMove += StickyNote_MouseMove;
            note.MouseLeftButtonUp += StickyNote_MouseLeftButtonUp;

            // Wiggle on hover
            note.MouseEnter += (s, e) =>
            {
                if (!_isDraggingNote)
                {
                    var wiggle = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(300) };
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(angle + 6, TimeSpan.FromMilliseconds(75)));
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(angle - 6, TimeSpan.FromMilliseconds(150)));
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(angle + 3, TimeSpan.FromMilliseconds(225)));
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(angle, TimeSpan.FromMilliseconds(300)));
                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, wiggle);
                }
            };

            StickyNoteCanvas.Children.Add(note);

            // Pop-in animation
            var easeFn = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 5 };
            popScale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(450)) { EasingFunction = easeFn });
            popScale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(450)) { EasingFunction = easeFn });
        }

        private void RestoreStickyNote(SavedNote noteData)
        {
            Color bgColor;
            try { bgColor = (Color)ColorConverter.ConvertFromString(noteData.Color); }
            catch { bgColor = Color.FromArgb(255, 250, 250, 200); }

            var note = new Border
            {
                Background = new SolidColorBrush(bgColor),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Width = 60,
                Height = 60,
                Padding = new Thickness(4),
                Tag = noteData.Text
            };

            var textBlock = new TextBlock
            {
                Text = noteData.Text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Comic Sans MS"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            note.Child = textBlock;

            var contextMenu = new ContextMenu();
            var editMenuItem = new MenuItem { Header = "編集 (Edit)" };
            editMenuItem.Click += (s, e) => EditStickyNote(note, textBlock);
            
            var completeMenuItem = new MenuItem { Header = "達成 (Complete)" };
            completeMenuItem.Click += (s, e) => {
                double noteX = Canvas.GetLeft(note) + 30;
                double noteY = Canvas.GetTop(note) + 30;
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(noteData.Text);
                AddPoints(10);
                PlayConfettiAnimation(noteX, noteY);
                SaveData();
            };

            var deleteMenuItem = new MenuItem { Header = "削除 (Delete)" };
            deleteMenuItem.Click += (s, e) => {
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(noteData.Text);
                SaveData();
            };

            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(completeMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            note.ContextMenu = contextMenu;

            note.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 3,
                ShadowDepth = 2,
                Opacity = 0.5
            };

            var restoredRotate = new RotateTransform(noteData.Angle);
            var transform = new TransformGroup();
            transform.Children.Add(restoredRotate);
            note.RenderTransform = transform;
            note.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(note, noteData.X);
            Canvas.SetTop(note, noteData.Y);

            // Re-enable hit test so user can click/right-click it
            note.IsHitTestVisible = true;

            // Register Drag and Drop events
            note.MouseLeftButtonDown += StickyNote_MouseLeftButtonDown;
            note.MouseMove += StickyNote_MouseMove;
            note.MouseLeftButtonUp += StickyNote_MouseLeftButtonUp;

            // Wiggle on hover
            double savedAngle = noteData.Angle;
            note.MouseEnter += (s, e) =>
            {
                if (!_isDraggingNote)
                {
                    var wiggle = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(300) };
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(savedAngle + 6, TimeSpan.FromMilliseconds(75)));
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(savedAngle - 6, TimeSpan.FromMilliseconds(150)));
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(savedAngle + 3, TimeSpan.FromMilliseconds(225)));
                    wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(savedAngle, TimeSpan.FromMilliseconds(300)));
                    restoredRotate.BeginAnimation(RotateTransform.AngleProperty, wiggle);
                }
            };

            StickyNoteCanvas.Children.Add(note);
        }

        private void StickyNote_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border note)
            {
                _isDraggingNote = true;
                _draggingNote = note;
                
                // Set the element to top ZIndex
                Panel.SetZIndex(note, StickyNoteCanvas.Children.Count);
                
                // Get starting position relative to the Canvas
                _dragStartMousePos = e.GetPosition(StickyNoteCanvas);
                _dragStartNotePos = new Point(Canvas.GetLeft(note), Canvas.GetTop(note));
                
                // Capture the mouse so it doesn't leave the element while dragging rapidly
                note.CaptureMouse();
                
                // We want to stop the main Window from dragging if we clicked a sticky note
                e.Handled = true;
            }
        }

        private void StickyNote_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingNote && _draggingNote != null && sender == _draggingNote)
            {
                // Calculate new position based on the delta from start
                Point currentMousePos = e.GetPosition(StickyNoteCanvas);
                
                double dx = currentMousePos.X - _dragStartMousePos.X;
                double dy = currentMousePos.Y - _dragStartMousePos.Y;
                
                double newLeft = _dragStartNotePos.X + dx;
                double newTop = _dragStartNotePos.Y + dy;
                
                // Restrict movement to within the StickyNoteCanvas (which matches window size)
                // Assuming note size is 60x60
                newLeft = Math.Max(0, Math.Min(StickyNoteCanvas.ActualWidth - _draggingNote.Width, newLeft));
                newTop = Math.Max(0, Math.Min(StickyNoteCanvas.ActualHeight - _draggingNote.Height, newTop));

                Canvas.SetLeft(_draggingNote, newLeft);
                Canvas.SetTop(_draggingNote, newTop);
            }
        }

        private void StickyNote_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingNote && _draggingNote != null && sender == _draggingNote)
            {
                _isDraggingNote = false;
                _draggingNote.ReleaseMouseCapture();
                _draggingNote = null;
                
                // Save positions after dragging finishes
                SaveData();
                e.Handled = true;
            }
        }

        private void EditStickyNote(Border note, TextBlock textBlock)
        {
            var textBox = new TextBox
            {
                Text = textBlock.Text,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true, // 複数行を許容するがEnter単押しで確定させたい
                Background = Brushes.White, // 視認性向上のため編集時は白背景
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Blue,
                FontSize = 10,
                FontFamily = new FontFamily("Comic Sans MS")
            };

            string originalText = textBlock.Text;
            bool isCommitted = false;

            RoutedEventHandler commitEdit = null;
            commitEdit = (s, e) =>
            {
                if (isCommitted) return;
                isCommitted = true;

                // イベントの解除
                textBox.LostFocus -= commitEdit;

                // 親(note)にフォーカスを戻す
                Keyboard.Focus(note);

                textBlock.Text = textBox.Text;
                note.Tag = textBox.Text;
                note.Child = textBlock; // 元のTextBlockに戻す
                UpdateTodoInList(originalText, textBox.Text);
                SaveData();
            };

            textBox.LostFocus += commitEdit;

            // PreviewKeyDown を使用して TextBox に消費される前に Enter と Esc を捕捉する
            textBox.PreviewKeyDown += (s, e) =>
            {
                // Shift+Enter は改行として許可し、Enter 単体は編集確定とする
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    e.Handled = true; // TextBox の改行処理をキャンセル
                    commitEdit(null, null);
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    if (!isCommitted)
                    {
                        isCommitted = true;
                        textBox.LostFocus -= commitEdit;
                        Keyboard.Focus(note);
                        note.Child = textBlock; // 変更を破棄して戻す
                    }
                }
            };

            note.Child = textBox;
            
            // 確実なフォーカス移行のため少し遅延させる
            Dispatcher.BeginInvoke(new Action(() => {
                textBox.Focus();
                Keyboard.Focus(textBox);
                textBox.SelectAll();
            }), DispatcherPriority.Background);
        }

        private void RemoveTodoFromList(string text)
        {
            var itemToRemove = Todos.FirstOrDefault(t => t.Text == text);
            if (itemToRemove != null)
            {
                Todos.Remove(itemToRemove);
            }
        }

        private void UpdateTodoInList(string oldText, string newText)
        {
            var item = Todos.FirstOrDefault(t => t.Text == oldText);
            if (item != null)
            {
                item.Text = newText;
                // Force UI update for the list box item
                var index = Todos.IndexOf(item);
                Todos[index] = item;
            }
        }

        private void Todo_Checked(object sender, RoutedEventArgs e)
        {
            // Add points when checked
            AddPoints(10);
            PlayConfettiAnimation(185, 230);
            
            var cb = sender as CheckBox;
            if (cb != null && cb.DataContext is TodoItem item)
            {
                // Remove item after a short delay
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    Todos.Remove(item);
                    
                    // Also remove the corresponding sticky note
                    var noteToRemove = StickyNoteCanvas.Children.OfType<Border>()
                                       .FirstOrDefault(b => b.Tag?.ToString() == item.Text);
                    if (noteToRemove != null)
                    {
                        StickyNoteCanvas.Children.Remove(noteToRemove);
                        SaveData();
                    }
                };
                timer.Start();
            }
        }

        private void Todo_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void AddPoints(int points)
        {
            _points += points;
            UpdatePointsDisplay();

            // Small animation on time to indicate point gain
            if (pixelCanvasTime != null)
            {
                var st = new ScaleTransform(1, 1);
                pixelCanvasTime.RenderTransform = st;
                pixelCanvasTime.RenderTransformOrigin = new Point(0.5, 0.5);

                var animX = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(100)) { AutoReverse = true };
                var animY = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(100)) { AutoReverse = true };
                st.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            }

            ShowFloatingPoints(points);
        }

        private void UpdatePointsDisplay()
        {
            if (pixelCanvasPoints != null)
                DrawPixelText(pixelCanvasPoints, $"{_points}", 1, new SolidColorBrush(Color.FromArgb(255, 255, 166, 64)));
        }

        // ─── Roguelike: コンボ ────────────────────────────────
        private double GetComboMultiplier() => _comboStreak switch
        {
            1 => 1.2,  2 => 1.5,  3 => 2.0,  >= 4 => 3.0,  _ => 1.0
        };

        private void UpdateComboDisplay()
        {
            if (pixelCanvasCombo == null) return;
            if (_comboStreak > 0)
            {
                Color c = _comboStreak >= 4
                    ? Color.FromArgb(255, 255, 80, 255)
                    : Color.FromArgb(255, 255, 160, 40);
                DrawPixelText(pixelCanvasCombo, $"C:{_comboStreak}", 1, new SolidColorBrush(c));
                if (pixelCanvasCombo.Effect is DropShadowEffect sh) sh.Color = c;
                pixelCanvasCombo.Visibility = Visibility.Visible;
            }
            else
            {
                pixelCanvasCombo.Children.Clear();
                pixelCanvasCombo.Visibility = Visibility.Hidden;
            }
        }

        // ─── Roguelike: スキル ────────────────────────────────
        private void UpdateSkillDisplay()
        {
            if (pixelCanvasSkill == null) return;
            pixelCanvasSkill.Children.Clear();
            if (_skillGuardActive || _skillSurgeActive)
            {
                string text = (_skillGuardActive && _skillSurgeActive) ? "GS"
                            : _skillGuardActive ? "G" : "S";
                Color c = (_skillGuardActive && _skillSurgeActive)
                    ? Color.FromArgb(255, 200, 230, 255)
                    : _skillGuardActive
                    ? Color.FromArgb(255, 100, 200, 255)
                    : Color.FromArgb(255, 255, 210, 50);
                DrawPixelText(pixelCanvasSkill, text, 1, new SolidColorBrush(c));
                pixelCanvasSkill.Visibility = Visibility.Visible;
            }
            else
            {
                pixelCanvasSkill.Visibility = Visibility.Hidden;
            }
        }

        private void SetupSkillContextMenu()
        {
            var menu = new ContextMenu();

            var guardItem = new MenuItem { Header = "🛡 守護盾 — 次スピン無料 (30pt)" };
            guardItem.Click += (s, e) =>
            {
                if (_skillGuardActive) { ShowEventNotification("守護盾はすでにアクティブ"); return; }
                if (_points >= 30) { _points -= 30; _skillGuardActive = true;
                    UpdatePointsDisplay(); UpdateSkillDisplay();
                    ShowEventNotification("🛡 守護盾 発動！\n次のスピンは無料！"); SaveData(); }
                else ShowEventNotification($"ポイント不足\n(30pt必要)");
            };

            var surgeItem = new MenuItem { Header = "⚡ 倍増 — 次の勝利2倍 (20pt)" };
            surgeItem.Click += (s, e) =>
            {
                if (_skillSurgeActive) { ShowEventNotification("倍増はすでにアクティブ"); return; }
                if (_points >= 20) { _points -= 20; _skillSurgeActive = true;
                    UpdatePointsDisplay(); UpdateSkillDisplay();
                    ShowEventNotification("⚡ 倍増 発動！\n次の勝利が2倍に！"); SaveData(); }
                else ShowEventNotification($"ポイント不足\n(20pt必要)");
            };

            var relicItem = new MenuItem { Header = "📿 所持レリック一覧" };
            relicItem.Click += (s, e) =>
            {
                if (_ownedRelics.Count == 0)
                    ShowEventNotification("レリック未取得\n(5勝ごとに1個獲得)");
                else
                    ShowEventNotification(string.Join("\n",
                        _ownedRelics.Select(r => GetRelicDisplayName(r))));
            };

            var statusItem = new MenuItem { Header = "📊 ステータス確認" };
            statusItem.Click += (s, e) =>
            {
                int ceiling = _ownedRelics.Contains("DIVINE_FAVOR") ? 75 : 100;
                int toNext = ceiling - (_totalSpinsCounter % ceiling);
                ShowEventNotification(
                    $"総スピン: {_totalSpinsCounter}\n" +
                    $"総勝利: {_totalWinCount}\n" +
                    $"天井まで: {toNext}回\n" +
                    $"コンボ: {_comboStreak}");
            };

            menu.Items.Add(guardItem);
            menu.Items.Add(surgeItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(relicItem);
            menu.Items.Add(statusItem);
            ScreenGrid.ContextMenu = menu;
        }

        // ─── Roguelike: レリック ──────────────────────────────
        private void UpdateRelicDisplay()
        {
            if (pixelCanvasRelics == null) return;
            pixelCanvasRelics.Children.Clear();
            var colors = new Dictionary<string, Color>
            {
                {"GOLDEN_FINGER",  Colors.Gold},
                {"LUCKY_CHARM",    Colors.MediumPurple},
                {"THRIFTY_WALLET", Colors.SteelBlue},
                {"DIVINE_FAVOR",   Colors.Orange},
                {"CHAIN_PROOF",    Colors.Crimson},
                {"MAGIC_DICE",     Colors.Cyan},
                {"DOUBLE_UP",      Colors.LimeGreen},
                {"PITY_GUARD",     Colors.HotPink},
            };
            int x = 0;
            foreach (var relic in _ownedRelics)
            {
                if (!colors.TryGetValue(relic, out Color c)) continue;
                var dot = new Rectangle { Width = 5, Height = 5,
                    Fill = new SolidColorBrush(c),
                    ToolTip = GetRelicDisplayName(relic) };
                Canvas.SetLeft(dot, x); Canvas.SetTop(dot, 0);
                pixelCanvasRelics.Children.Add(dot);
                x += 7;
            }
        }

        private string GetRelicDisplayName(string relic) => relic switch
        {
            "GOLDEN_FINGER"  => "🌟 黄金の指: 勝利ボーナス+50%",
            "LUCKY_CHARM"    => "🍀 幸運のお守り: ラッキー確率+10%",
            "THRIFTY_WALLET" => "💰 倹約家の財布: コスト-2pt",
            "DIVINE_FAVOR"   => "✨ 天運の加護: 天井75回",
            "CHAIN_PROOF"    => "🔗 連鎖の証: コンボ倍率+50%",
            "MAGIC_DICE"     => "🎲 魔法のサイコロ: 10%無料スピン",
            "DOUBLE_UP"      => "⬆ ダブルアップ: 15%でボーナス2倍",
            "PITY_GUARD"     => "🛡 加護の盾: 0pt時に15pt復活",
            _ => relic
        };

        private void GrantRandomRelic()
        {
            string[] avail = ALL_RELICS.Where(r => !_ownedRelics.Contains(r)).ToArray();
            if (avail.Length == 0)
            {
                ShowEventNotification("✦ 全レリック取得済み！\nさすが！");
                return;
            }
            string newRelic = avail[_random.Next(avail.Length)];
            _ownedRelics.Add(newRelic);
            UpdateRelicDisplay();
            PlayRelicGetAnimation(newRelic);
            SaveData();
        }

        private void PlayRelicGetAnimation(string relic)
        {
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Gold;
                    flashAnim.Begin();
                    var reset = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    reset.Tick += (s, e) => { FlashOverlay.Fill = Brushes.White; reset.Stop(); };
                    reset.Start();
                }
                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();
                ShowEventNotification($"★ レリック獲得！\n{GetRelicDisplayName(relic)}");
            }
            catch { }
        }

        // ─── イベント通知 ─────────────────────────────────────
        private void ShowEventNotification(string message)
        {
            try
            {
                if (OverlayCanvas == null) return;
                var panel = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(210, 20, 10, 40)),
                    BorderBrush = new SolidColorBrush(Colors.Gold),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(7, 4, 7, 4),
                    IsHitTestVisible = false
                };
                var text = new TextBlock
                {
                    Text = message,
                    Foreground = new SolidColorBrush(Colors.Gold),
                    FontFamily = new FontFamily("Comic Sans MS"),
                    FontSize = 9,
                    TextAlignment = TextAlignment.Center,
                    IsHitTestVisible = false
                };
                panel.Child = text;
                Canvas.SetLeft(panel, 95);
                Canvas.SetTop(panel, 70);
                OverlayCanvas.Children.Add(panel);
                var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                panel.BeginAnimation(Canvas.TopProperty,
                    new DoubleAnimation(70, 45, TimeSpan.FromMilliseconds(2500)) { EasingFunction = ease });
                var opAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(2500));
                opAnim.Completed += (s, e) => OverlayCanvas.Children.Remove(panel);
                panel.BeginAnimation(UIElement.OpacityProperty, opAnim);
            }
            catch { }
        }

        private void StartTodoButtonPulse()
        {
            try
            {
                var st = new ScaleTransform(1, 1);
                btnToggleTodo.RenderTransform = st;
                btnToggleTodo.RenderTransformOrigin = new Point(0.5, 0.5);
                var ease = new SineEase { EasingMode = EasingMode.EaseInOut };
                var sx = new DoubleAnimation(1.0, 1.25, TimeSpan.FromMilliseconds(850))
                    { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = ease };
                var sy = new DoubleAnimation(1.0, 1.25, TimeSpan.FromMilliseconds(850))
                    { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = ease };
                st.BeginAnimation(ScaleTransform.ScaleXProperty, sx);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, sy);
            }
            catch { }
        }

        private void PlayMinuteChimeAnimation()
        {
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = new SolidColorBrush(Color.FromArgb(100, 74, 255, 112));
                    flashAnim.Begin();
                    var reset = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    reset.Tick += (s, e) => { FlashOverlay.Fill = Brushes.White; reset.Stop(); };
                    reset.Start();
                }
            }
            catch { }
        }

        private void PlayConfettiAnimation(double originX = 185, double originY = 175)
        {
            try
            {
                if (OverlayCanvas == null) return;
                var confettiColors = new[]
                {
                    Colors.Gold, Colors.HotPink, Colors.Cyan, Colors.LimeGreen,
                    Colors.Orange, Colors.Violet, Colors.DeepSkyBlue, Colors.Yellow
                };

                for (int i = 0; i < 28; i++)
                {
                    UIElement shape = _random.Next(2) == 0
                        ? (UIElement)new Rectangle
                            {
                                Width = _random.Next(4, 10), Height = _random.Next(4, 10),
                                Fill = new SolidColorBrush(confettiColors[_random.Next(confettiColors.Length)])
                            }
                        : (UIElement)new Ellipse
                            {
                                Width = _random.Next(4, 9), Height = _random.Next(4, 9),
                                Fill = new SolidColorBrush(confettiColors[_random.Next(confettiColors.Length)])
                            };

                    double tx = originX + _random.Next(-90, 90);
                    double ty = originY - _random.Next(20, 130);

                    Canvas.SetLeft(shape, originX);
                    Canvas.SetTop(shape, originY);
                    OverlayCanvas.Children.Add(shape);

                    var dur = TimeSpan.FromMilliseconds(_random.Next(600, 1300));
                    var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                    shape.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(originX, tx, dur) { EasingFunction = ease });
                    shape.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(originY, ty, dur) { EasingFunction = ease });

                    var opAnim = new DoubleAnimation(1.0, 0.0, dur);
                    var capturedShape = shape;
                    opAnim.Completed += (s, e) => OverlayCanvas.Children.Remove(capturedShape);
                    shape.BeginAnimation(UIElement.OpacityProperty, opAnim);
                }
            }
            catch { }
        }

        private void ShowFloatingPoints(int points)
        {
            try
            {
                if (OverlayCanvas == null) return;
                double startX = 240;
                double startY = 225;

                var label = new TextBlock
                {
                    Text = $"+{points}",
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
                    FontFamily = new FontFamily("Comic Sans MS"),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(label, startX);
                Canvas.SetTop(label, startY);
                OverlayCanvas.Children.Add(label);

                var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                label.BeginAnimation(Canvas.TopProperty,
                    new DoubleAnimation(startY, startY - 55, TimeSpan.FromMilliseconds(1200)) { EasingFunction = ease });

                var opAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(1200));
                opAnim.Completed += (s, e) => OverlayCanvas.Children.Remove(label);
                label.BeginAnimation(UIElement.OpacityProperty, opAnim);
            }
            catch { }
        }
    }

    public class TodoItem
    {
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class AppData
    {
        public int Points { get; set; }
        public List<SavedNote> Notes { get; set; } = new List<SavedNote>();
        public string TvFrameColor { get; set; } = "#333333";
        public string ScreenGridColor { get; set; } = "#FF0D3A20";
        public double TvOuterRadius { get; set; } = 8.0;
        public double TvInnerRadius { get; set; } = 8.0;
        public int SlimeLevel { get; set; } = 0;
        public int TotalSpins { get; set; } = 0;
        public int ComboStreak { get; set; } = 0;
        public List<string> OwnedRelics { get; set; } = new List<string>();
        public int TotalWinCount { get; set; } = 0;
        public bool SkillGuardActive { get; set; } = false;
        public bool SkillSurgeActive { get; set; } = false;
    }

    public class SavedNote
    {
        public string Text { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public string Color { get; set; }
    }
}
