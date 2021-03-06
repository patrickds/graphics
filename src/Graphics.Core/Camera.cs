﻿using Graphics.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphics.Core
{
    public class Camera : Entity
    {
        public Camera(double near, double far, double aspectRatio)
        {
            this.Position = new Vector4(50 , 50, -350, 1);
            this.Target = Vector4.Zero;

            // 110 degrees, close to human FoV
            this.FoV = 1.919862d;

            this.Near = near;
            this.Far = far;
            _aspectRatio = aspectRatio;

            this.Calculate();   
        }

        private double _aspectRatio = 1d;
        public Vector4 Target { get; set; }
        public Vector4 Gaze { get; set; }
        public Vector4 Up { get; set; }
        public Vector4 Right { get; set; }
        public double FoV { get; set; }
        public double Near { get; set; }
        public double Far { get; set; }
        public Vector3 LeftBottomNear { get; set; }
        public Vector3 RightTopFar { get; set; }

        private void Calculate()
        {
            this.CalculateAxis();
            this.CalculateViewFrustrum();
        }

        private void CalculateAxis()
        {
            this.Gaze = (this.Target - this.Position).Normalize();
            this.Right = this.Gaze.Cross(Vector4.J).Normalize();
            this.Up = this.Right.Cross(this.Gaze).Normalize();
        }

        private void CalculateViewFrustrum()
        {
            double height = System.Math.Tan(this.FoV / 2d) * this.Near;
            double width = height * _aspectRatio;

            double halfWidth = width / 2d;
            double halfHeight = height / 2d;

            this.LeftBottomNear = new Vector3(-halfWidth, -halfHeight, this.Near);
            this.RightTopFar = new Vector3(halfWidth, halfHeight, this.Far);

            //this.CalculateNearPlane();
            //this.CalculateFarPlane();
            //this.CalculateFrustrum();
        }

        private void CalculateNearPlane()
        {
            var angle = this.FoV / 2;
            var halfHeight = (System.Math.Tan(angle) * this.Near);
            var height = halfHeight * 2;
            var width = height * _aspectRatio;
            var halfWidth = width /2;

            var nearZ = ((this.Gaze * this.Near) + this.Position).Z;

            var bottomLeftX = (this.Right * (halfWidth)).X;
            var bottomLeftY = (this.Up * (-halfHeight)).Y;

            var bottomRightX = (this.Right * (-halfWidth)).X;
            var bottomRightY = (this.Up * (-halfHeight)).Y;

            var topLeftX = (this.Right * (halfWidth)).X;
            var topLeftY = (this.Up * (halfHeight)).Y;

            var topRightX = (this.Right * (-halfWidth)).X;
            var topRightY = (this.Up * (halfHeight)).Y;

            this.LeftBottomNear = new Vector3(bottomLeftX, bottomLeftY, nearZ);
        }

        private void CalculateFarPlane()
        {
            var angle = this.FoV / 2;
            var halfHeight = (System.Math.Tan(angle) * this.Far);
            var height = halfHeight * 2;
            var width = height * _aspectRatio;
            var halfWidth = width / 2;

            var farZ = ((this.Gaze * this.Far) + this.Position).Z;

            var bottomLeftX = (this.Right * (halfWidth)).X;
            var bottomLeftY = (this.Up * (-halfHeight)).Y;

            var bottomRightX = (this.Right * (-halfWidth)).X;
            var bottomRightY = (this.Up * (-halfHeight)).Y;

            var topLeftX = (this.Right * (halfWidth)).X;
            var topLeftY = (this.Up * (halfHeight)).Y;

            var topRightX = (this.Right * (-halfWidth)).X;
            var topRightY = (this.Up * (halfHeight)).Y;
            
            this.RightTopFar = new Vector3(topRightX, topRightY, farZ);
        }

        private void CalculateFrustrum()
        {
            var angle = this.FoV / 2;
            var Hnear = 2 * System.Math.Tan(angle) * this.Near;
            var Wnear = Hnear * _aspectRatio;

            var Hfar = 2 * System.Math.Tan(angle) * this.Far;
            var Wfar = Hfar * _aspectRatio;

            var Cnear = this.Position + this.Gaze * this.Near;
            var Cfar = this.Position + this.Gaze * this.Far;

            var NearTopLeft = Cnear + (this.Up * (Hnear / 2d)) - (this.Right * (Wnear / 2d));
            var NearTopRight = Cnear + (this.Up * (Hnear / 2d)) + (this.Right * (Wnear / 2d));
            var NearBottomLeft = Cnear - (this.Up * (Hnear / 2d)) - (this.Right * (Wnear /2d));
            var NearBottomRight = Cnear - (this.Up * (Hnear / 2d)) + (this.Right * (Wnear / 2d));
            var FarTopLeft = Cfar + (this.Up * (Hfar / 2d)) - (this.Right * (Wfar / 2d));
            var FarTopRight = Cfar + (this.Up * (Hfar / 2d)) + (this.Right * (Wfar / 2d));
            var FarBottomLeft = Cfar - (this.Up * (Hfar / 2d)) - (this.Right * (Wfar / 2d));
            var FarBottomRight = Cfar - (this.Up * (Hfar / 2d)) + (this.Right * (Wfar / 2d));

            this.LeftBottomNear = NearBottomLeft.ToVector3();
            this.RightTopFar = FarTopRight.ToVector3();
        }

        internal void Zoom(double factor)
        {
            var zoom = this.Gaze * factor;
            var translationVector = this.Right + this.Up + zoom;

            this.Position += translationVector;
            this.Target += translationVector;
        }

        internal void Tranlate(Vector4 translation)
        {
            var xTranslation = this.Right * -translation.X;
            var yTranslation = this.Up * -translation.Y;
            var zTranslation = this.Gaze * translation.Z;

            var translationVector = xTranslation + yTranslation + zTranslation;

            this.Position += translationVector;
            this.Target += translationVector;
        }

        internal void Rotate(double xRadians, double yRadians)
        {
            var xRotation = Matrix4.CreateRotation(this.Up.ToVector3(), xRadians);
            var yRotation = Matrix4.CreateRotation(this.Right.ToVector3(), -yRadians);

            this.Position = xRotation * yRotation * this.Position;

            this.CalculateAxis();
        }

        public void UpdateAspectRatio(double aspectRatio)
        {
            _aspectRatio = aspectRatio;
            this.CalculateViewFrustrum();
        }
    }
}