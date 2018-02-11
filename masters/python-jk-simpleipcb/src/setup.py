from setuptools import setup


def readme():
	with open('README.rst') as f:
		return f.read()


setup(name = "jk_simpleipcb",
	version = "0.2018.2.11",
	description = "This module provides a way to attach other processes (possibly written in other programming languages) to the current python process in order to use functionality provided in these other processes.",
	author = "Jürgen Knauth",
	author_email = "pubsrc@binary-overflow.de",
	license = "Apache 2.0",
	url = "https://github.com/jkpubsrc/simpleipcb/masters/python-module-jk-simpleipcb",
	download_url = "https://github.com/jkpubsrc/simpleipcb/tarball/0.2018.2.11",
	keywords = [ 'ipc', 'communication', 'process', 'multiprocessing' ],
	packages = [
		"jk_simpleipcb"
	],
	install_requires = [
	],
	include_package_data = True,
	classifiers = [
		"Development Status :: 4 - Beta",
		"Programming Language :: Python :: 3",
		"License :: OSI Approved :: Apache Software License",
		"Topic :: System :: Distributed Computing",
		"Operating System :: POSIX :: Linux",
	],
	long_description = readme(),
	zip_safe = False)





