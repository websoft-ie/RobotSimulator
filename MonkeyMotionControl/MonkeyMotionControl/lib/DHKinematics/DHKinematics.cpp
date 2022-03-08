//
// Copyright (c) 2012, Andre Gaschler
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

#include <iostream>
#include <memory>
#include <stdexcept>
#include <random>

#include <rl/kin/Kinematics.h>
#include <rl/kin/Puma.h>
#include <rl/math/Unit.h>


extern "C" __declspec(dllexport) int rlForwardKinematics(char* xmlFile, double camera_center_offset, double j1, double  j2, double j3, double  j4, double  j5, double j6, double* outputs)
// camera_center_offset : offset constant of camera center away from flange (This offset point is used as reference point of camera coordinate)
{
	std::string path = xmlFile;
	try
	{
		std::shared_ptr<rl::kin::Kinematics> kinematics(rl::kin::Kinematics::create(path, camera_center_offset));

		rl::math::Vector q(6);
		q[0] = j1 * rl::math::DEG2RAD;
		q[1] = j2 * rl::math::DEG2RAD;
		q[2] = j3 * rl::math::DEG2RAD;
		q[3] = j4 * rl::math::DEG2RAD;
		q[4] = j5 * rl::math::DEG2RAD;
		q[5] = j6 * rl::math::DEG2RAD;

		kinematics->setPosition(q);
		kinematics->updateFrames();
		rl::math::Transform t = kinematics->forwardPosition();

		Eigen::Affine3d::TranslationPart translate = t.translation();
		Eigen::Matrix3d rotate = t.rotation();
		Eigen::Vector3d ea = rotate.eulerAngles(2, 1, 0);

		*outputs = translate.x();
		*(outputs + 1) = translate.y();
		*(outputs + 2) = translate.z();
		*(outputs + 3) = ea[2] * rl::math::RAD2DEG;
		*(outputs + 4) = ea[1] * rl::math::RAD2DEG;
		*(outputs + 5) = ea[0] * rl::math::RAD2DEG;
	}
	catch (const std::exception & e)
	{
		std::cerr << e.what() << std::endl;
		return EXIT_FAILURE;
	}

	return 0;
}

extern "C" __declspec(dllexport) int rlInverseKinematics(char* xmlFile, double camera_center_offset, double* prev, double x, double y, double z, double rotX, double rotY, double rotZ, double* outputs)
// camera_center_offset : offset constant of camera center away from flange (This offset point is used as reference point of camera coordinate)
{
	std::string path = xmlFile;
	std::shared_ptr<rl::kin::Kinematics> kinematics(rl::kin::Kinematics::create(path, camera_center_offset));

	rl::math::Vector qzero(kinematics->getDof());
	for (std::size_t i = 0; i < 6; ++i)
		//qzero(i) = 0;
		qzero(i) = *(prev + i) * rl::math::DEG2RAD;

	kinematics->setPosition(qzero);
	kinematics->updateFrames();
	rl::math::Transform t_1 = kinematics->forwardPosition();

	rl::math::Vector qinv(kinematics->getDof());

	rl::math::Transform t;
	t.setIdentity();

	t = ::rl::math::AngleAxis(rotZ * ::rl::math::DEG2RAD, ::rl::math::Vector3::UnitZ())
		* ::rl::math::AngleAxis(rotY * ::rl::math::DEG2RAD, ::rl::math::Vector3::UnitY())
		* ::rl::math::AngleAxis(rotX * ::rl::math::DEG2RAD, ::rl::math::Vector3::UnitX());

	//t.translation().x() = x;
	//t.translation().y() = y;
	//t.translation().z() = z;

	t.translation()(0) = x;
	t.translation()(1) = y;
	t.translation()(2) = z;
	rl::math::Vector q(kinematics->getDof());
	kinematics->getPosition(q);

	if ((q - qzero).norm() > 0.001)
		printf("Error!");

	if (dynamic_cast<::rl::kin::Kinematics*>(kinematics.get())->inversePosition(t, qinv, 0, 1.0f))
		for (int i = 0; i < 6; i++)
			outputs[i] = qinv[i] * rl::math::RAD2DEG;
	else
		for (int i = 0; i < 6; i++)
			outputs[i] = q[i] * rl::math::RAD2DEG;

	return 0;
}



