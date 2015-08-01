# Makefile for building PersistentThrust

KSPDIR	:= ${HOME}/.local/share/Steam/steamapps/common/Kerbal Space Program
MANAGED	:= ${KSPDIR}/KSP_Data/Managed/
SRCFILES := src/Extensions.cs \
	src/SolarSailPart.cs \
	src/Utils.cs \
	src/PersistentEngine.cs
GMCS	:= gmcs
TAR	:= tar
ZIP	:= zip

all: build

info:
	@echo "== PersistentThrust Build Information =="
	@echo "  gmcs:    ${GMCS}"
	@echo "  tar:     ${TAR}"
	@echo "  zip:     ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "==========================================="

build: build/PersistentThrust.dll

build/%.dll: ${SRCFILES}
	mkdir -p build
	${GMCS} -t:library -lib:"${MANAGED}" -lib:"${MECHJEB}" \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:$@ \
		${SRCFILES}

package: build ${SRCFILES}
	mkdir -p package/PersistentThrust/Plugins
	cp -r Parts package/PersistentThrust/
	cp -r Patches package/PersistentThrust/
	cp build/PersistentThrust.dll package/PersistentThrust/Plugins/
	cp License.md README.org TODO.org CHANGELOG.org package/PersistentThrust/

%.tgz:
	cd package; ${TAR} zcf ../$@ PersistentThrust

tgz: package SolarSailNavigator.tgz

%.zip:
	cd package; ${ZIP} -9 -r ../$@ PersistentThrust

zip: package PersistentThrust.zip

clean:
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: build
	mkdir -p "${KSPDIR}"/GameData/PersistentThrust/Plugins
	cp -r Parts "${KSPDIR}"/GameData/PersistentThrust/
	cp -r Patches "${KSPDIR}"/GameData/PersistentThrust/
	cp build/PersistentThrust.dll "${KSPDIR}"/GameData/PersistentThrust/Plugins/

uninstall: info
	rm -rf "${KSPDIR}"/GameData/PersistentThrust/

.PHONY : all info build package tar.gz zip clean install uninstall
