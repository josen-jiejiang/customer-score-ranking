# Customers Score Ranking

#### Introduce
**Develop a small HTTP-based back-end service which stores and
 provides scores and ranks for customers**

####  Business Model

1.  Customer:Each customer is identified by an unique CustomerID, which is an int64 number. In this service, any arbitrary number can identify a customer.
2.  Score:Eachcustomer hasascore, which is a decimal number. Thedefault score of a customer is zero.
3.  Leaderboard:All customers whose score is greater than zero participate in a competition. Each
 customer is associated with an unique rank in the leaderboard, determined by their scores.
<br>The with the highest score is at rank 1
<br>The customer with the second highest score is at rank 2
<br>...
4.  Two customers with the same score, their ranksare determined by their CustomerID, lower is first.
5.  When customer’s score changes,its rankinleaderboard is affected in realtime.

####  Nonfunctional Requirements

1.  Nopersistence nor database is required. State in memory only.
2.  Only use .NetCore.Do notuseexternal frameworks/assemblies, except maybe for testing.
3.  Theservice needstobeable to handlelots of simultaneous requests.
4.  Theservice needstobeable to handlelots of customers, so bear in mind the complexity, especially the time complexity of frequent operations.
5.  Createarepository in github.com, and upload your source code there.

####  Functional Requirements

1.  Update Score
2.  Get customers by rank
3.  Get customers by customerid

#### Involving Technology

1.  ILogger
2.  IMemoryCache
3.  ChannelContext
4.  List<<span>T</span>>
5.  Thread 

#### Implementation Logic 

1.  Update Score 
<br>1.1. Obtain user information, including scores.
<br>1.2. Additional score, when less than 0, defaults to 0.
<br>1.3. Asynchronous writing of current user information into an unbounded queue for score ranking.
<br>1.4. Return updated data.

2.  Get customers by rank
<br>2.1. Basic parameter verification, the starting ranking must be lower than the ending ranking.
<br>2.2. Obtain user cache information and obtain user rankings.
<br>2.3. Verify whether there is user information in the current ranking, and if there is, obtain an index based on the ranking and calculate the required number of rows to be retrieved.
<br>2.4. Return the current user ranking results.


3.  Get customers by customerid
<br>3.1. Obtain user ranking information
<br>3.2. Retrieve user by customer ID, return if not present
<br>3.3. Obtain the index in the ranking based on the current user information, and calculate the required number of ranking rows according to the ranking.
<br>3.4. Return the current user ranking results.