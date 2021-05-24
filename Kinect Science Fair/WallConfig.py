import csv

# with open('Depth Data/KinectDataMinMax.csv', 'r', newline='') as file:
#     reader = csv.reader(file)
#     fileWriter = open("Depth Data/KinectDataMinMaxWallConfig.csv", "w")
#     count = 0
#     for row in reader:
#         target = row.pop(0) #Remove the target
#         if target == "Wall":
#             row = list(map(int, row))
#             for elem in row:
#                 if elem > 0.6:
#                     count+=1
#             if count > 300:
#                 target = "Wall with Floor"
#             fileWriter.write(target + ", " + str(row).strip('[]') + "\n")
#     fileWriter.close()
        

with open('Depth Data/KinectData.csv', 'r', newline='') as file:
    reader = csv.reader(file)
    with open('Depth Data/KinectDataMinMax.csv', 'r', newline='') as file2:
        reader2 = list(csv.reader(file2))
        fileWriter = open("Depth Data/KinectDataWallConfig.csv", "w")
        count = 0
        for row in reader:
            target = row.pop(0) #Remove the target
            row = list(map(int, row))
            reader2[count].pop(0)
            rowMinMax = list(map(float, reader2[count]))
            if target == "Wall":
                maxList = max(row)
                minList = min(i for i in row if i > 0)
                if (minList + 500 > maxList and rowMinMax[0] < 0.25):
                    target = "Wall without Floor"
                else:
                    target = "Wall with Floor"
            fileWriter.write(target + ", " + str(row).strip('[]') + "\n")
            count+=1
        fileWriter.close()
        
