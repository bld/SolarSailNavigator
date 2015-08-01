# Makefile for building SolarSailNavigator

KSPDIR	:= ${HOME}/.local/share/Steam/steamapps/common/Kerbal Space Program
MANAGED	:= ${KSPDIR}/KSP_Data/Managed/
PT	:= PersistentThrust/build/
MM	:= ${KSPDIR}/GameData/
SSFILES	:= src/Navigator.cs \
	src/Controls.cs \
	src/Preview.cs \
	src/SolarSailControlled.cs \
	src/SailControls.cs \
	src/SailPreview.cs \
	src/Frames.cs
GMCS	:= gmcs
TAR	:= tar
ZIP	:= zip

all: 	build

ptbuild:
	cd PersistentThrust; $(MAKE) build

ptinstall:
	cd PersistentThrust; $(MAKE) install

ptuninstall:
	cd PersistentThrust; $(MAKE) uninstall

ptpackage: ptbuild
	cd PersistentThrust; $(MAKE) package

ptclean:
	cd PersistentThrust; $(MAKE) clean

info:
	@echo "== SolarSailNavigator Build Information =="
	@echo "  gmcs:     ${GMCS}"
	@echo "  tar:      ${TAR}"
	@echo "  zip:      ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "  PT:       ${PT}"
	@echo "================================"

build: ptbuild build/SolarSailNavigator.dll

build/%.dll: ${SSFILES}
	mkdir -p build
	${GMCS} -t:library -lib:"${MANAGED}" -lib:"${PT}" -lib:"${MM}" \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine,PersistentThrust \
		-out:$@ \
		${SSFILES}

package: ptpackage build ${SSFILES}
	mkdir -p package/SolarSailNavigator/Plugins
	cp build/SolarSailNavigator.dll package/SolarSailNavigator/Plugins/
	cp -r Patches package/SolarSailNavigator
	cp License.md README.org TODO.org CHANGELOG.org package/SolarSailNavigator/
	cp -r PersistentThrust/package/PersistentThrust package/

%.tgz:
	cd package; ${TAR} zcf ../$@ SolarSailNavigator

tgz: package SolarSailNavigator.tgz

%.zip:
	cd package; ${ZIP} -9 -r ../$@ SolarSailNavigator PersistentThrust

zip: package SolarSailNavigator.zip

clean: ptclean
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: ptinstall build
	mkdir -p "${KSPDIR}"/GameData/SolarSailNavigator/Plugins
	cp build/SolarSailNavigator.dll "${KSPDIR}"/GameData/SolarSailNavigator/Plugins/
	cp -r Patches "${KSPDIR}"/GameData/SolarSailNavigator/

uninstall: ptuninstall info
	rm -rf "${KSPDIR"/GameData/SolarSailNavigator/Plugins

.PHONY : all info build package tar.gz zip clean install uninstall
