using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;

namespace SkelematorPipeline
{
    [ContentProcessor(DisplayName = "Skelemator HDR Texture Processor")]
    public class HDRTextureProcessor : ContentProcessor<List<byte[]>, Texture2DContent>
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public override Texture2DContent Process(List<byte[]> input, ContentProcessorContext context)
        {
            for (int p = 0; p < input.Count; ++p)
            {
                if (input[p].Length != Height * Width * sizeof(float) * 3 / (1 << (2 * p)))
                    throw new InvalidContentException("The number of bytes in one or more of the images does not correlate with the product of the Height and Width properties.");
            }

            MipmapChain imageChain = new MipmapChain();
            int mip = 0;
            for (; mip < input.Count; ++mip)
            {
                byte[] paddedBytes = new byte[input[mip].Length / 3 * 4];
                int srcIndex = 0;
                int destIndex = 0;
                while (srcIndex < input[mip].Length)
                {
                    paddedBytes[destIndex++] = input[mip][srcIndex++];
                    if (srcIndex % 12 == 0)
                    {
                        for (int x = 0; x < 4; ++x)
                        {
                            paddedBytes[destIndex++] = 0;
                        }
                    }
                }

                int mipReduction = 1 << mip;
                BitmapContent image = new PixelBitmapContent<Vector4>(Width / mipReduction, Height / mipReduction);
                image.SetPixelData(paddedBytes);
                imageChain.Add(image);
            }

            // Check to see if this is a partial mipmap chain:
            if (imageChain.Count > 1)
            {
                // Just fill the rest of the chain with anything to satisfy the validator that the chain is complete.
                while ((Math.Max(Height, Width) >> (mip - 1)) > 1)
                {
                    int mipReduction = 1 << mip;
                    int mipHeight = Math.Max(Height / mipReduction, 1);
                    int mipWidth = Math.Max(Width / mipReduction, 1);
                    byte[] bytes = new byte[mipHeight * mipWidth * sizeof(float) * 4];
                    BitmapContent image = new PixelBitmapContent<Vector4>(mipWidth, mipHeight);
                    image.SetPixelData(bytes);
                    imageChain.Add(image);
                    ++mip;
                }
            }

            Texture2DContent outputTC = new Texture2DContent();
            outputTC.Mipmaps = imageChain;

            return outputTC;
        }
    }
}