#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
from selenium import webdriver
from bs4 import BeautifulSoup
import getpass
import requests
from selenium.webdriver.common.keys import Keys
import pprint
from colorama import Fore, Back, Style

#           !!! ERROR de sign out yaptır
#           BİTİŞTE sign out yaptır
#           Zorla kapatmada izin verme veya ...
#           Problemli girişi html içerikle tespit edip uyar
#           pv-position-entity
#           pv-education-entity
'''

'''

def main():
    try:
        userid = str(input("Enter email address or number with country code: "))
        password = str(input('Enter your password:'))
        chrome_path = 'PATH_OF chromedriver.exe'
        driver = webdriver.Chrome(chrome_path)
        driver.get("https://www.linkedin.com/login?trk=guest_homepage-basic_nav-header-signin")
        driver.implicitly_wait(6)
        driver.find_element_by_xpath('//*[@id="username"]').send_keys(userid)
        driver.find_element_by_xpath('//*[@id="password"]').send_keys(password)
        driver.find_element_by_xpath('//*[@type="submit"]').click()

        userList = [
            "https://www.linkedin.com/in/fatih-islamoglu-11892a6/",
            "https://www.linkedin.com/in/esen-girit-t%C3%BCmer-b99a706/",
            "https://www.linkedin.com/in/altan-demirdere-08a56915a/",
            "https://www.linkedin.com/in/zuhtusoylu/"]
        for user in userList:
            driver.get(user) #Enter any of your connection profile Link
            #allInfo = driver.find_elements_by_class_name('pv-entity__summary-info')#.get_attribute('innerHTML')
            showMore = driver.find_elements_by_class_name('pv-profile-section__text-truncate-toggle')
            if len(showMore) == 0:
                print("\t Nothing more found")
            else:
                for more in showMore:
                    buttons = ["more role","more education","more experience"]
                    if(len([butt for butt in buttons if butt in more.text]) > 0 ):
                        more.click()

            userInfo = driver.find_elements_by_xpath("//*[contains(@class,'pv-entity__summary-info')]")
            eduInfo  = driver.find_elements_by_class_name('pv-education-entity')
            expInfo  = driver.find_elements_by_class_name('pv-position-entity')
            print("=========================================================================")
            print("\n\t\t" + user.split('/')[4] + "\n")
            userFile = open(user.split('/')[4] + ".txt",'w',encoding='utf-8')

            print(" |\n | EDUCATION:______________________________________________________________________________")
            userFile.write(" |\n | EDUCATION:______________________________________________________________________________")
            for edu1 in eduInfo:
                attr = edu1.text.split("\n")
                fieldIndex  = attr.index("Field Of Study") if "Field Of Study" in attr else -1
                field = attr[fieldIndex+1] if fieldIndex != -1 else "??"
                dateIndex   = attr.index("Dates attended or expected graduation") if "Dates attended or expected graduation" in attr else -1
                schoolName  = attr[0]
                newEdu = Edu(schoolName,attr[dateIndex+1],field)
                userFile.write(newEdu.eprint())

            print("\n | EXP:______________________________________________________________________________")
            userFile.write("\n | EXP:______________________________________________________________________________")
            for exp in expInfo:
                print("\t" + exp.text.replace("\n","\n\t"))
                userFile.write("\t" + exp.text.replace("\n","\n\t"))
            #driver.find_element_by_css_selector('button.contact-see-more-less').click()
            userFile.close()

    except Exception as err:
        #if("3200" in str(err.msg) and "Cannot navigate to invalid URL" in str(err.msg)):
        #    print("\nProbably invalid Credentials !")
        #else:
        #    print(err.msg)
        print(str(err))
        input("\nPress any to continue . . . ")

main()