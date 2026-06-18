using System;
using ArchdruidsAdditions.Data;
using DevInterface;

namespace ArchdruidsAdditions.Objects.DevObjects
{
    public class KeyboardInput : PositionedDevUINode
    {
        public float titleWidth;
        public KeyBoardInputBox inputBox;

        public float InputPosCoord
        {
            get
            {
                return titleWidth + 10f;
            }
        }

        public string Value
        {
            get
            {
                return inputBox.input;
            }
        }

        public KeyboardInput(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
            : base(owner, IDstring, parentNode, pos)
        {
            titleWidth = 110f;

            subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), titleWidth, title));

            inputBox = new KeyBoardInputBox(owner, "InputBox", this, new Vector2(InputPosCoord, 0), new Vector2(240f - InputPosCoord, 16f));
            subNodes.Add(inputBox);
        }

        public override void Refresh()
        {
            base.Refresh();
        }

        public class KeyBoardInputBox : RectangularDevUINode
        {
            public FSprite whiteSquare;
            public FSprite blackSquare;
            public float borderWidth;

            public FLabel inputLabel;
            public string input;
            public int flashUnderscoreTimer;

            public int typeCooldown;

            public bool clicked;

            public static KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode)) as KeyCode[];

            public bool validString;

            public KeyBoardInputBox(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size)
            : base(owner, IDstring, parentNode, pos, size)
            {
                borderWidth = 1f;
                input = "INPUT NAME";

                whiteSquare = new("pixel", true)
                {
                    scaleY = size.y,
                    scaleX = size.x,
                    anchorX = 0f,
                    anchorY = 0f,
                    color = new Color(1f, 1f, 1f),
                    alpha = 0.5f
                };
                fSprites.Add(whiteSquare);

                blackSquare = new("pixel", true)
                {
                    scaleY = whiteSquare.scaleY - (borderWidth * 2),
                    scaleX = whiteSquare.scaleX - (borderWidth * 2),
                    anchorX = 0f,
                    anchorY = 0f,
                    color = new Color(0f, 0f, 0f)
                };
                fSprites.Add(blackSquare);

                inputLabel = new(Custom.GetFont(), input)
                {
                    alignment = FLabelAlignment.Left,
                    anchorX = 0,
                    anchorY = 0,
                    color = Color.white,
                    alpha = 0.5f,
                    scale = 1f
                };
                fLabels.Add(inputLabel);

                if (owner != null)
                {
                    Futile.stage.AddChild(whiteSquare);
                    Futile.stage.AddChild(blackSquare);
                    Futile.stage.AddChild(inputLabel);
                }
            }

            public override void Update()
            {
                base.Update();

                if (owner != null && owner.mouseClick)
                {
                    if (MouseOver)
                    {
                        clicked = true;

                        PlayerData.TextBeingInputted = true;
                    }
                    else if (clicked)
                    {
                        clicked = false;

                        PlayerData.TextBeingInputted = false;
                    }
                }

                if (clicked)
                {
                    if (flashUnderscoreTimer > 100)
                    { flashUnderscoreTimer = 0; }
                    else
                    { flashUnderscoreTimer++; }

                    if (flashUnderscoreTimer < 50)
                    { inputLabel.text = input + "_"; }
                    else
                    { inputLabel.text = input; }
                }
                else
                {
                    inputLabel.text = input;
                    flashUnderscoreTimer = 0;
                }
            }

            public override void Refresh()
            {
                base.Refresh();

                if (owner != null)
                {
                    whiteSquare.SetPosition(absPos);
                    blackSquare.SetPosition(absPos + new Vector2(borderWidth, borderWidth));
                    inputLabel.SetPosition(absPos + new Vector2(borderWidth + 2, borderWidth + 1));

                    whiteSquare.alpha = (MouseOver || clicked) ? 1f : 0.5f;
                    inputLabel.alpha = clicked ? 1f : 0.5f;

                    if (!clicked)
                    {
                        if (validString)
                        {
                            SetColor(Color.green);
                        }
                        else
                        {
                            SetColor(Color.red);
                        }
                    }
                    else
                    {
                        {
                            SetColor(Color.white);
                        }
                    }
                }

                if (clicked && Input.anyKey && Input.inputString.Length > 0)
                {
                    foreach (char c in Input.inputString)
                    {
                        if (c == '\b')
                        {
                            if (input.Length > 0)
                            {
                                input = input.Remove(input.Length - 1, 1);
                            }
                        }
                        else if (c == '\n' || c == '\r')
                        {
                            clicked = false;

                            PlayerData.TextBeingInputted = false;
                        }
                        else
                        {
                            input += c;
                        }
                    }
                }
            }

            public void SetColor(Color color)
            {
                whiteSquare.color = color;
                inputLabel.color = color;
            }
        }
    }

    public class CustomSlider : Slider
    {
        public DevUILabel title;
        public DevUILabel number;
        public Button button;
        public SliderNub nub;

        public float numberWidth;

        new public float SliderStartCoord
        {
            get
            {
                return titleWidth + 10f + numberWidth + 4f + (inheritButton ? 34f : 0f); 
            }
        }

        public float SliderWidth
        {
            get
            {
                return 240f - SliderStartCoord;
            }
        }

        public CustomSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, bool inheritButton, float titleWidth, float numberWidth)
            : base(owner, IDstring, parentNode, pos, title, inheritButton, titleWidth)
        {
            this.title = subNodes[0] as DevUILabel;
            number = subNodes[1] as DevUILabel;

            this.numberWidth = numberWidth;
            number.size = new Vector2(numberWidth, 16f);
            number.fSprites[0].scaleX = numberWidth;

            if (inheritButton)
            {
                button = subNodes[2] as Button;
                nub = subNodes[3] as SliderNub;

                button.size = new Vector2(numberWidth, 16f);
                button.fSprites[0].scaleX = numberWidth;
            }
            else
            {
                nub = subNodes[2] as SliderNub;
            }

            fSprites[0].scaleX = SliderWidth;
            fSprites[1].scaleX = SliderWidth;
        }

        public override void Update()
        {
            base.Update();

            if (owner != null && nub.held)
            {
                NubDragged(Mathf.InverseLerp(absPos.x + SliderStartCoord, absPos.x + SliderStartCoord + SliderWidth - 8f, owner.mousePos.x + nub.mousePosOffset));
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            MoveSprite(0, absPos + new Vector2(SliderStartCoord, 0f));
            MoveSprite(1, absPos + new Vector2(SliderStartCoord, 7f));
        }

        new public void RefreshNubPos(float nubPos)
        {
            nub.Move(new Vector2(Mathf.Lerp(SliderStartCoord, SliderStartCoord + SliderWidth - 8f, nubPos), 0f));
        }
    }
}
