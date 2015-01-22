using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class MatrixStack
    {
        private Stack<Matrix> mStack;
        private Matrix mCurrent;
        private Matrix mCurrentInverse;

        public MatrixStack()
        {
            mStack = new Stack<Matrix>();
            mCurrent = Matrix.Identity;
            mCurrentInverse = Matrix.Identity;
        }

        public void Push()
        {
            mStack.Push(mCurrent);
        }

        public void Pop()
        {
            mCurrent = mStack.Peek();
            mCurrentInverse = Matrix.Invert(mCurrent);
            mStack.Pop();
        }

        public void MultMatrixLocal(Matrix a)
        {
            mCurrent = a * mCurrent;
            mCurrentInverse = Matrix.Invert(mCurrent);
        }

        public Matrix Top
        {
            get { return mCurrent; }
        }

        public Matrix TopInverse
        {
            get { return mCurrentInverse; }
        }

        public void Clear()
        {
            mStack.Clear();
            mCurrent = Matrix.Identity;
            mCurrentInverse = Matrix.Identity;
        }
    }
}
